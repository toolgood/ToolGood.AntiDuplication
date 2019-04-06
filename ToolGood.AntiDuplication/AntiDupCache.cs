using System;
using System.Collections.Generic;
using System.Threading;

namespace ToolGood.AntiDuplication
{
    /// <summary>
    /// 防重复缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AntiDupCache<TKey, TValue>
    {
        private readonly int _maxCount;//缓存最高数量
        private readonly long _expireTicks;//超时 Ticks
        private long _lastTicks;//最后Ticks
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _slimLock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, Tuple<long, TValue>> _map = new Dictionary<TKey, Tuple<long, TValue>>();
        private readonly Dictionary<TKey, AntiDupLockSlim> _lockDict = new Dictionary<TKey, AntiDupLockSlim>();
        private readonly Queue<TKey> _queue = new Queue<TKey>();


        /// <summary>
        /// 防重复缓存
        /// </summary>
        /// <param name="maxCount">缓存最高数量</param>
        /// <param name="expireSecond">超时秒数</param>
        public AntiDupCache(int maxCount = 100, int expireSecond = 1)
        {
            _maxCount = maxCount;
            _expireTicks = expireSecond * TimeSpan.FromSeconds(1).Ticks;
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
            if (object.Equals(null, key) || _maxCount <= 0) {
                return factory();
            }

            Tuple<long, TValue> tuple;
            AntiDupLockSlim slim = null;
            long lastTicks;

            _lock.EnterReadLock();
            try {
                if (_map.TryGetValue(key, out tuple)) {
                    if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) { return tuple.Item2; }
                }
                lastTicks = _lastTicks;
            } finally {
                _lock.ExitReadLock();
            }

            _slimLock.EnterUpgradeableReadLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out tuple)) {
                        if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) { return tuple.Item2; }
                    }
                } finally {
                    _lock.ExitReadLock();
                }
                _slimLock.EnterWriteLock();
                if (_lockDict.TryGetValue(key, out slim) == false) {
                    slim = new AntiDupLockSlim();
                    _lockDict[key] = slim;
                }
                slim.UseCount++;
                _slimLock.ExitWriteLock();

            } finally {
                _slimLock.ExitUpgradeableReadLock();
            }

            slim.EnterWriteLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out tuple)) {
                        if (tuple.Item1 + _expireTicks > DateTime.Now.Ticks) {
                            return tuple.Item2;
                        }
                    }
                } finally {
                    _lock.ExitReadLock();
                }

                var val = factory();
                _lock.EnterWriteLock();
                _lastTicks = DateTime.Now.Ticks;
                _map[key] = Tuple.Create(_lastTicks, val);
                if (_queue.Contains(key) == false) {
                    _queue.Enqueue(key);
                    if (_queue.Count > _maxCount) {
                        _map.Remove(_queue.Dequeue());
                    }
                }
                _lock.ExitWriteLock();
                return val;
            } finally {
                slim.ExitWriteLock();
                _slimLock.EnterWriteLock();
                slim.UseCount--;
                if (slim.UseCount == 0) {
                    _lockDict.Remove(key);
                    slim.Dispose();
                }
                _slimLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            // Cache it
            _lock.EnterWriteLock();
            try {
                _map.Clear();
                _lockDict.Clear();
                _queue.Clear();
            } finally {
                _lock.ExitWriteLock();
            }
        }

    }

}
