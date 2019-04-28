using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ToolGood.AntiDuplication
{
    /// <summary>
    /// 防重复缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AntiDupCache<TKey, TValue> : IExecuteCache<TKey, TValue>
    {
        private const int _thousand = 1000;
        private readonly int _maxCount;//缓存最高数量
        private readonly long _expireTicks;//超时 Ticks
        private long _lastTicks;//最后Ticks
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _slimLock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, Tuple<long, TValue>> _map = new Dictionary<TKey, Tuple<long, TValue>>();
        private readonly Dictionary<TKey, AntiDupLockSlim> _lockDict = new Dictionary<TKey, AntiDupLockSlim>();
        private readonly Queue<TKey> _queue = new Queue<TKey>();
        class AntiDupLockSlim : ReaderWriterLockSlim { public int UseCount; }

        /// <summary>
        /// 防重复缓存
        /// </summary>
        /// <param name="maxCount">缓存最高数量,0 不缓存，-1 缓存所有</param>
        /// <param name="expireSecond">超时秒数,0 不缓存，-1 永久缓存 </param>
        public AntiDupCache(int maxCount = 100, int expireSecond = 1)
        {
            if (maxCount < 0) {
                _maxCount = -1;
            } else {
                _maxCount = maxCount;
            }
            if (expireSecond < 0) {
                _expireTicks = -1;
            } else {
                _expireTicks = expireSecond * TimeSpan.FromSeconds(1).Ticks;
            }
        }

        /// <summary>
        /// 个数
        /// </summary>
        public int Count {
            get { return _map.Count; }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        public TValue Execute(TKey key, Func<TValue> factory)
        {
            // 过期时间为0 则不缓存
            if (object.Equals(null, key) || _expireTicks == 0L || _maxCount == 0) { return factory(); }

            Tuple<long, TValue> tuple;
            long lastTicks;
            _lock.EnterReadLock();
            try {
                if (_map.TryGetValue(key, out tuple)) {
                    if (_expireTicks == -1) return tuple.Item2;
                    if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) return tuple.Item2;
                }
                lastTicks = _lastTicks;
            } finally { _lock.ExitReadLock(); }


            AntiDupLockSlim slim;
            _slimLock.EnterUpgradeableReadLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_lastTicks != lastTicks) {
                        if (_map.TryGetValue(key, out tuple)) {
                            if (_expireTicks == -1) return tuple.Item2;
                            if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) return tuple.Item2;
                        }
                        lastTicks = _lastTicks;
                    }
                } finally { _lock.ExitReadLock(); }

                _slimLock.EnterWriteLock();
                try {
                    if (_lockDict.TryGetValue(key, out slim) == false) {
                        slim = new AntiDupLockSlim();
                        _lockDict[key] = slim;
                    }
                    slim.UseCount++;
                } finally { _slimLock.ExitWriteLock(); }
            } finally { _slimLock.ExitUpgradeableReadLock(); }


            slim.EnterWriteLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out tuple)) {
                        if (_expireTicks == -1) return tuple.Item2;
                        if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) return tuple.Item2;
                    }
                } finally { _lock.ExitReadLock(); }

                var val = factory();
                _lock.EnterWriteLock();
                try {
                    _lastTicks = DateTime.Now.Ticks;
                    _map[key] = Tuple.Create(_lastTicks, val);
                    if (_maxCount > 0) {
                        if (_queue.Contains(key) == false) {
                            _queue.Enqueue(key);
                            if (_queue.Count > _maxCount) _map.Remove(_queue.Dequeue());
                        }
                    }
                } finally { _lock.ExitWriteLock(); }
                return val;
            } finally {
                slim.ExitWriteLock();
                _slimLock.EnterWriteLock();
                try {
                    slim.UseCount--;
                    if (slim.UseCount == 0) {
                        _lockDict.Remove(key);
                        slim.Dispose();
                    }
                } finally { _slimLock.ExitWriteLock(); }
            }
        }


        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="secord">每次超时秒数，最多8次</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        public TValue Execute(TKey key, int secord, Func<TValue> factory)
        {
            // 过期时间为0 则不缓存
            if (object.Equals(null, key) || _expireTicks == 0L || _maxCount == 0) { return factory(); }

            Tuple<long, TValue> tuple;
            long lastTicks;
            _lock.TryEnterReadLock(secord * _thousand);
            try {
                if (_map.TryGetValue(key, out tuple)) {
                    if (_expireTicks == -1) return tuple.Item2;
                    if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) return tuple.Item2;
                }
                lastTicks = _lastTicks;
            } finally { _lock.ExitReadLock(); }


            AntiDupLockSlim slim;
            _slimLock.TryEnterUpgradeableReadLock(secord * _thousand);
            try {
                _lock.TryEnterReadLock(secord * _thousand);
                try {
                    if (_lastTicks != lastTicks) {
                        if (_map.TryGetValue(key, out tuple)) {
                            if (_expireTicks == -1) return tuple.Item2;
                            if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) return tuple.Item2;
                        }
                        lastTicks = _lastTicks;
                    }
                } finally { _lock.ExitReadLock(); }

                _slimLock.TryEnterWriteLock(secord * _thousand);
                try {
                    if (_lockDict.TryGetValue(key, out slim) == false) {
                        slim = new AntiDupLockSlim();
                        _lockDict[key] = slim;
                    }
                    slim.UseCount++;
                } finally { _slimLock.ExitWriteLock(); }
            } finally { _slimLock.ExitUpgradeableReadLock(); }


            slim.TryEnterWriteLock(secord * _thousand);
            try {
                _lock.TryEnterReadLock(secord * _thousand);
                try {
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out tuple)) {
                        if (_expireTicks == -1) return tuple.Item2;
                        if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) return tuple.Item2;
                    }
                } finally { _lock.ExitReadLock(); }

                var val = factory();
                _lock.TryEnterWriteLock(secord * _thousand);
                try {
                    _lastTicks = DateTime.Now.Ticks;
                    _map[key] = Tuple.Create(_lastTicks, val);
                    if (_maxCount > 0) {
                        if (_queue.Contains(key) == false) {
                            _queue.Enqueue(key);
                            if (_queue.Count > _maxCount) _map.Remove(_queue.Dequeue());
                        }
                    }
                } finally { _lock.ExitWriteLock(); }
                return val;
            } finally {
                slim.ExitWriteLock();
                _slimLock.TryEnterWriteLock(secord * _thousand);
                try {
                    slim.UseCount--;
                    if (slim.UseCount == 0) {
                        _lockDict.Remove(key);
                        slim.Dispose();
                    }
                } finally { _slimLock.ExitWriteLock(); }
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try {
                _map.Clear();
                _queue.Clear();
                _slimLock.EnterWriteLock();
                try {
                    _lockDict.Clear();
                } finally {
                    _slimLock.ExitWriteLock();
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }

    }

}
