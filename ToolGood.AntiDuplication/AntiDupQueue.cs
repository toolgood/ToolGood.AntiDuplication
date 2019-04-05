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
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, ReaderWriterLockSlim> _lockDict = new Dictionary<TKey, ReaderWriterLockSlim>();
        private readonly Queue<TKey> _queue = new Queue<TKey>();


        /// <summary>
        /// 防重复列队
        /// </summary>
        /// <param name="maxCount">缓存最高数量</param>
        public AntiDupQueue(int maxCount = 100)
        {
            _maxCount = maxCount;
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
            ReaderWriterLockSlim slim;
            try {
                if (_map.TryGetValue(key, out tuple)) {
                    return tuple;
                }
                _lockDict.TryGetValue(key, out slim);
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
                        return tuple;
                    }
                } finally {
                    _lock.ExitReadLock();
                }

                var val = factory();

                _lock.EnterWriteLock();
                try {
                    _map[key] = val;
                    if (_lockDict.ContainsKey(key) == false) {
                        _lockDict[key] = slim;
                    }
                    _queue.Enqueue(key);
                    if (_queue.Count > _maxCount) {
                        var oldKey = _queue.Dequeue();
                        _map.Remove(oldKey);
                        _lockDict.Remove(oldKey);
                    }
                } finally {
                    _lock.ExitWriteLock();
                }

                return val;
            } finally {
                slim.ExitWriteLock();
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
