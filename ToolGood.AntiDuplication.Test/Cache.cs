using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolGood.AntiDuplication.QueryApi
{
    internal class Cache<TKey, TValue>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        public int Count {
            get { return _map.Count; }
        }

        public TValue Execute(TKey key, Func<TValue> factory)
        {
            // Check cache
            _lock.EnterReadLock();
            TValue val;
            try {
                if (_map.TryGetValue(key, out val))
                    return val;
            } finally {
                _lock.ExitReadLock();
            }

            // Cache it
            _lock.EnterWriteLock();
            try {
                // Check again
                if (_map.TryGetValue(key, out val))
                    return val;

                // Create it
                val = factory();

                // Store it
                _map.Add(key, val);

                // Done
                return val;
            } finally {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            // Cache it
            _lock.EnterWriteLock();
            try {
                _map.Clear();
            } finally {
                _lock.ExitWriteLock();
            }
        }
    }

}
