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
        private readonly int _expireSecond;//超时秒数
        private readonly long _sencondTicks;// 1秒的Ticks值
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, Tuple<long, TValue>> _map = new Dictionary<TKey, Tuple<long, TValue>>();
        private readonly Dictionary<TKey, ReaderWriterLockSlim> _lockDict = new Dictionary<TKey, ReaderWriterLockSlim>();
        private readonly Queue<TKey> _queue = new Queue<TKey>();

        /// <summary>
        /// 防重复缓存
        /// </summary>
        /// <param name="maxCount">缓存最高数量</param>
        /// <param name="expireSecond">超时秒数</param>
        public AntiDupCache(int maxCount = 100, int expireSecond = 1)
        {
            _maxCount = maxCount;
            _expireSecond = expireSecond;
            _sencondTicks = TimeSpan.FromSeconds(1).Ticks;
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
            ReaderWriterLockSlim slim = null;

            _lock.EnterReadLock();
            try {
                if (_map.TryGetValue(key, out tuple)) {
                    if (tuple.Item1 + _expireSecond * _sencondTicks > DateTime.Now.Ticks) {
                        return tuple.Item2;
                    }
                    _lockDict.TryGetValue(key, out slim);
                }
            } finally {
                _lock.ExitReadLock();
            }

            if (slim == null) {
                _lock.EnterWriteLock();
                try {
                    if (_lockDict.TryGetValue(key, out slim) == false) {
                        slim = new ReaderWriterLockSlim();
                        _lockDict[key] = slim;
                    }
                } finally {
                    _lock.ExitWriteLock();
                }
            }

            slim.EnterWriteLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_map.TryGetValue(key, out tuple)) {
                        if (tuple.Item1 + _expireSecond * _sencondTicks > DateTime.Now.Ticks) {
                            return tuple.Item2;
                        }
                    }
                } finally {
                    _lock.ExitReadLock();
                }
        
                var val = factory();

                _lock.EnterWriteLock();
                try {
                    _map[key] = Tuple.Create(DateTime.Now.Ticks, val);
                    if (tuple == null) {
                        if (_lockDict.ContainsKey(key) == false) {
                            _lockDict[key] = slim;
                        }
                        _queue.Enqueue(key);
                        if (_queue.Count > _maxCount) {
                            var oldKey = _queue.Dequeue();
                            _map.Remove(oldKey);
                            _lockDict.Remove(oldKey);
                        }
                    }
                } finally {
                    _lock.ExitWriteLock();
                }

                return val;
            } finally {
                slim.ExitWriteLock();
            }
        }

        public void Flush()
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
