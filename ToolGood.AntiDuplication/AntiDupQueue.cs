using System;
using System.Collections.Generic;
using System.Threading;

namespace ToolGood.AntiDuplication
{
    /// <summary>
    /// 防重复列队
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AntiDupQueue<TKey, TValue>
    {
        private readonly int _maxCount;//缓存最高数量
        private long _lastTicks;//最后Ticks
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _slimLock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, AntiDupLockSlim> _lockDict = new Dictionary<TKey, AntiDupLockSlim>();
        private readonly Queue<TKey> _queue = new Queue<TKey>();

        //private readonly List<ReaderWriterLockSlim> _usedLockSlims = new List<ReaderWriterLockSlim>();

        /// <summary>
        /// 防重复列队
        /// </summary>
        /// <param name="maxCount">缓存最高数量，0或负数不缓存</param>
        public AntiDupQueue(int maxCount = 100)
        {
            _maxCount = maxCount;
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
            if (object.Equals(null, key) || _maxCount <= 0) { return factory(); }

            _lock.EnterReadLock();
            TValue tuple;
            AntiDupLockSlim slim;
            long lastTicks;
            try {
                if (_map.TryGetValue(key, out tuple)) { return tuple; }
                lastTicks = _lastTicks;
            } finally {
                _lock.ExitReadLock();
            }

            _slimLock.EnterUpgradeableReadLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out tuple)) { return tuple; }
                } finally {
                    _lock.ExitReadLock();
                }
                _slimLock.EnterWriteLock();
                try {
                    if (_lockDict.TryGetValue(key, out slim) == false) {
                        slim = new AntiDupLockSlim();
                        _lockDict[key] = slim;
                    }
                    slim.UseCount++;
                } finally {
                    _slimLock.ExitWriteLock();
                }
            } finally {
                _slimLock.ExitUpgradeableReadLock();
            }

            slim.EnterWriteLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_map.TryGetValue(key, out tuple)) { return tuple; }
                } finally {
                    _lock.ExitReadLock();
                }

                var val = factory();

                _lock.EnterWriteLock();
                try {
                    _map[key] = val;
                    _queue.Enqueue(key);
                    if (_queue.Count > _maxCount) {
                        var oldKey = _queue.Dequeue();
                        _map.Remove(oldKey);
                    }
                } finally {
                    _lock.ExitWriteLock();
                }

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
