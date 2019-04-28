using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ToolGood.AntiDuplication
{
    /// <summary>
    /// 字典缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictCache<TKey, TValue>
    {
        private const int _thousand = 1000;
        private long _lastTicks;//最后Ticks
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _slimLock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, AntiDupLockSlim> _lockDict = new Dictionary<TKey, AntiDupLockSlim>();
        class AntiDupLockSlim : ReaderWriterLockSlim { public int UseCount; }


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
        /// <returns></returns>
        public TValue Execute(TKey key, Func<TValue> factory)
        {
            if (object.Equals(key, null)) { return factory(); }

            long lastTicks;
            TValue val;
            _lock.EnterReadLock();
            try {
                if (_map.TryGetValue(key, out val)) return val;
                lastTicks = _lastTicks;
            } finally { _lock.ExitReadLock(); }

            AntiDupLockSlim slim;
            _slimLock.EnterUpgradeableReadLock();
            try {
                _lock.EnterReadLock();
                try {
                    if (_lastTicks != lastTicks) {
                        if (_map.TryGetValue(key, out val)) return val;
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
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out val)) return val;
                } finally { _lock.ExitReadLock(); }

                val = factory();
                _lock.EnterWriteLock();
                try {
                    _lastTicks = DateTime.Now.Ticks;
                    _map[key] = val;
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
            if (object.Equals(key, null)) { return factory(); }

            long lastTicks;
            TValue val;
            _lock.TryEnterReadLock(secord * _thousand);
            try {
                if (_map.TryGetValue(key, out val)) return val;
                lastTicks = _lastTicks;
            } finally { _lock.ExitReadLock(); }

            AntiDupLockSlim slim;
            _slimLock.TryEnterUpgradeableReadLock(secord * _thousand);
            try {
                _lock.TryEnterReadLock(secord * _thousand);
                try {
                    if (_lastTicks != lastTicks) {
                        if (_map.TryGetValue(key, out val)) return val;
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
                    if (_lastTicks != lastTicks && _map.TryGetValue(key, out val)) return val;
                } finally { _lock.ExitReadLock(); }

                val = factory();
                _lock.TryEnterWriteLock(secord * _thousand);
                try {
                    _lastTicks = DateTime.Now.Ticks;
                    _map[key] = val;
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
