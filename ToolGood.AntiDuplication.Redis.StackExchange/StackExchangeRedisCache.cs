﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using StackExchange.Redis;
//using Newtonsoft.Json;

//namespace ToolGood.AntiDuplication.Redis.StackExchange
//{
//    public class StackExchangeRedisCache<TKey, TValue> : IConcurrentCache<TKey, TValue>
//    {
//        private readonly string _rediesConnStr;
//        private readonly int _databaseId;
//        private readonly string _prefix;
//        private readonly string _lockPrefix;
//        private readonly int _timeout;

//        public StackExchangeRedisCache(string rediesConnStr, int databaseId, string prefix, string lockPrefix, int timeout)
//        {
//            _rediesConnStr = rediesConnStr;
//            _databaseId = databaseId;
//            _prefix = prefix;
//            _lockPrefix = lockPrefix;
//            _timeout = timeout;
//        }

//        #region 属性
//        /// <summary>
//        /// 获取缓存个数
//        /// </summary>
//        public int Count {
//            get {
//                return 0;
//            }
//        }

//        /// <summary>
//        /// 是否为空
//        /// </summary>
//        public bool IsEmpty {
//            get {
//                return 0;
//            }
//        }
//        #endregion

//        #region TValue

//        /// <summary>
//        /// 获取或添加缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <param name="val">值</param>
//        /// <returns></returns>
//        public TValue GetOrAdd(TKey key, TValue val, int secord = 0)
//        {
//            // 过期时间为0 则不缓存
//            if (object.Equals(null, key)) { return val; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) { return value; }

//            AntiDupLockSlim slim;
//            _slimLock.EnterUpgradeableReadLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                slim = addLock(key);
//            } finally { _slimLock.ExitUpgradeableReadLock(); }

//            slim.EnterWriteLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                trySet(key, val, secord);
//                return val;
//            } finally {
//                slim.ExitWriteLock();
//                removeLock(key, slim);
//            }
//        }

//        /// <summary>
//        /// 设置缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <param name="value">值</param>
//        public void SetValue(TKey key, TValue value, int secord = 0)
//        {
//            if (object.Equals(null, key)) return;
//            trySet(key, value, secord);
//        }

//        /// <summary>
//        /// 添加或更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="addValue">添加值</param>
//        /// <param name="updateValue">更新值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return addValue; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                trySet(key, updateValue);
//                return value;
//            }

//            AntiDupLockSlim slim;
//            _slimLock.EnterUpgradeableReadLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                slim = addLock(key);
//            } finally { _slimLock.ExitUpgradeableReadLock(); }

//            slim.EnterWriteLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                trySet(key, addValue, secord);
//                return addValue;
//            } finally {
//                slim.ExitWriteLock();
//                removeLock(key, slim);
//            }
//        }

//        /// <summary>
//        /// 尝试获取缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <param name="value">值</param>
//        /// <returns></returns>
//        public bool TryGetValue(TKey key, out TValue value, int secord = 0)
//        {
//            value = default(TValue);
//            if (object.Equals(null, key)) { return false; }

//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) { return true; }
//            return false;
//        }

//        /// <summary>
//        /// 尝试添加缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="value">值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryAdd(TKey key, TValue value, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord) == false) {
//                trySet(key, value, secord);
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="newValue">新值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryUpdate(TKey key, TValue newValue, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                trySet(key, newValue);
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="newValue">新值</param>
//        /// <param name="comparisonValue">原值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                if (object.Equals(value, comparisonValue)) {
//                    trySet(key, newValue);
//                }
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 尝试删除缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="value">值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryRemove(TKey key, out TValue value, int secord = 0)
//        {
//            value = default(TValue);
//            if (object.Equals(null, key)) { return false; }
//            return tryRemove(key, ref value, secord);
//        }
//        /// <summary>
//        /// 删除缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="secord">每次超时秒数</param>
//        public void Remove(TKey key, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return; }
//            tryRemove(key, secord);
//        }
//        /// <summary>
//        /// 是否包含缓存
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="secord"></param>
//        /// <returns></returns>
//        public bool ContainsKey(TKey key, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            return containsKey(key, secord);
//        }

//        #endregion

//        #region Func<TValue> factory
//        /// <summary>
//        /// 获取或添加缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <param name="factory">执行方法</param>
//        /// <returns></returns>
//        public TValue GetOrAdd(TKey key, Func<TValue> factory, int secord = 0)
//        {
//            // 过期时间为0 则不缓存
//            if (object.Equals(null, key)) { return factory(); }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) { return value; }

//            AntiDupLockSlim slim;
//            _slimLock.EnterUpgradeableReadLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                slim = addLock(key);
//            } finally { _slimLock.ExitUpgradeableReadLock(); }

//            slim.EnterWriteLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                var val = factory();
//                trySet(key, val, secord);
//                return val;
//            } finally {
//                slim.ExitWriteLock();
//                removeLock(key, slim);
//            }
//        }
//        /// <summary>
//        /// 设置缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="factory">执行方法</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public TValue SetValue(TKey key, Func<TValue> factory, int secord = 0)
//        {
//            if (object.Equals(null, key)) return factory();
//            var value = factory();
//            trySet(key, value, secord);
//            return value;
//        }
//        /// <summary>
//        /// 添加或更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="addValueFactory">添加的值</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public TValue AddOrUpdate(TKey key, Func<TValue> addValueFactory, Func<TValue> updateValueFactory, int secord)
//        {
//            if (object.Equals(null, key)) { return addValueFactory(); }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                value = updateValueFactory();
//                trySet(key, value);
//                return value;
//            }

//            AntiDupLockSlim slim;
//            _slimLock.EnterUpgradeableReadLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                slim = addLock(key);
//            } finally { _slimLock.ExitUpgradeableReadLock(); }

//            slim.EnterWriteLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                var val = addValueFactory();
//                trySet(key, val, secord);
//                return val;
//            } finally {
//                slim.ExitWriteLock();
//                removeLock(key, slim);
//            }
//        }
//        /// <summary>
//        /// 尝试添加缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="factory"></param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryAdd(TKey key, Func<TValue> factory, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }
//            var value = factory();
//            trySet(key, value, secord);
//            return true;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryUpdate(TKey key, Func<TValue> updateValueFactory, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                trySet(key, updateValueFactory(), secord);
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="comparisonValue">原值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public bool TryUpdate(TKey key, Func<TValue> updateValueFactory, TValue comparisonValue, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                if (object.Equals(value, comparisonValue)) {
//                    trySet(key, updateValueFactory(), secord);
//                }
//                return true;
//            }
//            return false;
//        }

//        #endregion

//        #region async Func<TValue> factory
//#if !NET40
//        /// <summary>
//        /// 获取或添加缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <param name="factory">执行方法</param>
//        /// <returns></returns>
//        public async Task<TValue> GetOrAdd(TKey key, Func<Task<TValue>> factory, int secord = 0)
//        {
//            // 过期时间为0 则不缓存
//            if (object.Equals(null, key)) { return await factory(); }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) { return value; }

//            AntiDupLockSlim slim;
//            _slimLock.EnterUpgradeableReadLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                slim = addLock(key);
//            } finally { _slimLock.ExitUpgradeableReadLock(); }

//            slim.EnterWriteLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                var val = await factory();
//                trySet(key, val, secord);
//                return val;
//            } finally {
//                slim.ExitWriteLock();
//                removeLock(key, slim);
//            }
//        }
//        /// <summary>
//        /// 设置缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="factory">执行方法</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public async Task<TValue> SetValue(TKey key, Func<Task<TValue>> factory, int secord = 0)
//        {
//            if (object.Equals(null, key)) return await factory();
//            var value = await factory();
//            trySet(key, value, secord);
//            return value;
//        }
//        /// <summary>
//        /// 添加或更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="addValueFactory">添加的值</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public async Task<TValue> AddOrUpdate(TKey key, Func<Task<TValue>> addValueFactory, Func<Task<TValue>> updateValueFactory, int secord)
//        {
//            if (object.Equals(null, key)) { return await addValueFactory(); }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                value = await updateValueFactory();
//                trySet(key, value);
//                return value;
//            }

//            AntiDupLockSlim slim;
//            _slimLock.EnterUpgradeableReadLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                slim = addLock(key);
//            } finally { _slimLock.ExitUpgradeableReadLock(); }

//            slim.EnterWriteLock();
//            try {
//                if (checkGet(key, lastTicks, ref value, secord)) { return value; }
//                var val = await addValueFactory();
//                trySet(key, val, secord);
//                return val;
//            } finally {
//                slim.ExitWriteLock();
//                removeLock(key, slim);
//            }
//        }
//        /// <summary>
//        /// 尝试添加缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="factory"></param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public async Task<bool> TryAdd(TKey key, Func<Task<TValue>> factory, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }
//            var value = await factory();
//            trySet(key, value, secord);
//            return true;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public async Task<bool> TryUpdate(TKey key, Func<Task<TValue>> updateValueFactory, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                trySet(key, await updateValueFactory(), secord);
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="comparisonValue">原值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public async Task<bool> TryUpdate(TKey key, Func<Task<TValue>> updateValueFactory, TValue comparisonValue, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                if (object.Equals(value, comparisonValue)) {
//                    trySet(key, await updateValueFactory(), secord);
//                }
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 尝试更新缓存
//        /// </summary>
//        /// <param name="key">关键字</param>
//        /// <param name="updateValueFactory">更新的值</param>
//        /// <param name="comparisonValueFactory">原值</param>
//        /// <param name="secord">每次超时秒数</param>
//        /// <returns></returns>
//        public async Task<bool> TryUpdate(TKey key, Func<Task<TValue>> updateValueFactory, Func<Task<TValue>> comparisonValueFactory, int secord = 0)
//        {
//            if (object.Equals(null, key)) { return false; }

//            TValue value = default(TValue);
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                if (object.Equals(value, await comparisonValueFactory())) {
//                    trySet(key, await updateValueFactory(), secord);
//                }
//                return true;
//            }
//            return false;
//        }

//#endif
//        #endregion

//        #region 清空
//        /// <summary>
//        /// 清空
//        /// </summary>
//        public void Clear()
//        {
//            _lock.EnterWriteLock();
//            try {
//                _map.Clear();
//                _slimLock.EnterWriteLock();
//                try {
//                    _lockDict.Clear();
//                } finally {
//                    _slimLock.ExitWriteLock();
//                }
//            } finally {
//                _lock.ExitWriteLock();
//            }
//        }
//        #endregion


//        #region private 方法
//        private bool tryGet(TKey key, ref TValue value, ref long lastTicks, int secord = 0)
//        {
//            if (secord == 0) {
//                _lock.EnterReadLock();
//            } else {
//                _lock.TryEnterReadLock(secord * _thousand);
//            }
//            try {
//                if (_map.TryGetValue(key, out value)) {
//                    return true;
//                }
//                lastTicks = _lastTicks;
//            } finally { _lock.ExitReadLock(); }
//            return false;
//        }

//        private bool checkGet(TKey key, long lastTicks, ref TValue value, int secord = 0)
//        {
//            if (secord == 0) {
//                _lock.EnterReadLock();
//            } else {
//                _lock.TryEnterReadLock(secord * _thousand);
//            }
//            try {
//                if (_lastTicks != lastTicks && _map.TryGetValue(key, out value)) {
//                    return true;
//                }
//            } finally { _lock.ExitReadLock(); }
//            return false;
//        }

//        private void trySet(TKey key, TValue value, int secord = 0)
//        {
//            if (secord == 0) {
//                _lock.EnterWriteLock();
//            } else {
//                _lock.TryEnterWriteLock(secord * _thousand);
//            }
//            try {
//                _map[key] = value;
//            } finally { _lock.ExitWriteLock(); }
//        }

//        private bool tryRemove(TKey key, ref TValue value, int secord = 0)
//        {
//            long lastTicks = 0;
//            if (tryGet(key, ref value, ref lastTicks, secord)) {
//                if (secord == 0) {
//                    _lock.EnterWriteLock();
//                } else {
//                    _lock.TryEnterWriteLock(secord * _thousand);
//                }
//                try {
//                    _map.Remove(key);
//                    return true;
//                } finally { _lock.ExitWriteLock(); }
//            }
//            return false;
//        }
//        private void tryRemove(TKey key, int secord = 0)
//        {
//            if (secord == 0) {
//                _lock.EnterWriteLock();
//            } else {
//                _lock.TryEnterWriteLock(secord * _thousand);
//            }
//            try {
//                _map.Remove(key);
//            } finally { _lock.ExitWriteLock(); }
//        }

//        private bool containsKey(TKey key, int secord = 0)
//        {
//            if (secord == 0) {
//                _lock.EnterReadLock();
//            } else {
//                _lock.TryEnterReadLock(secord * _thousand);
//            }
//            try {
//                return _map.ContainsKey(key);
//            } finally { _lock.ExitReadLock(); }
//        }

//        private AntiDupLockSlim addLock(TKey key)
//        {
//            AntiDupLockSlim slim;
//            _slimLock.EnterWriteLock();
//            try {
//                if (_lockDict.TryGetValue(key, out slim) == false) {
//                    slim = new AntiDupLockSlim();
//                    _lockDict[key] = slim;
//                }
//                slim.UseCount++;
//            } finally { _slimLock.ExitWriteLock(); }
//            return slim;
//        }

//        private void removeLock(TKey key, AntiDupLockSlim slim)
//        {
//            _slimLock.EnterWriteLock();
//            try {
//                slim.UseCount--;
//                if (slim.UseCount == 0) {
//                    _lockDict.Remove(key);
//                    slim.Dispose();
//                }
//            } finally { _slimLock.ExitWriteLock(); }
//        }
//        #endregion





//        /// <summary>
//        /// 执行
//        /// </summary>
//        /// <param name="key">值</param>
//        /// <param name="factory">执行方法</param>
//        /// <returns></returns>
//        public TValue Execute(TKey key, Func<TValue> factory)
//        {
//            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
//            IDatabase db = redis.GetDatabase(_databaseId);
//            var keyStr = _prefix + key.ToString();
//            var val = db.StringGet(keyStr);
//            if (val.IsNullOrEmpty) {
//                if (db.LockTake("lock." + keyStr, "1", TimeSpan.FromSeconds(_timeout * 2))) {
//                    val = db.StringGet(keyStr);
//                    if (val.IsNullOrEmpty) {
//                        var v = factory();
//                        var json = JsonConvert.SerializeObject(v);
//                        db.StringSet(keyStr, json, TimeSpan.FromSeconds(_timeout));
//                        db.LockRelease("lock." + keyStr, "1");
//                        return v;
//                    }
//                    db.LockRelease("lock." + keyStr, "1");
//                }
//            }
//            if (val.IsNullOrEmpty) { return default(TValue); }
//            var str = val.ToString();
//            return JsonConvert.DeserializeObject<TValue>(str);
//        }

//        /// <summary>
//        /// 执行
//        /// </summary>
//        /// <param name="key">值</param>
//        /// <param name="secord">每次超时秒数，最多8次</param>
//        /// <param name="factory">执行方法</param>
//        /// <returns></returns>
//        public TValue Execute(TKey key, int secord, Func<TValue> factory)
//        {
//            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
//            IDatabase db = redis.GetDatabase(_databaseId);
//            var keyStr = _prefix + key.ToString();
//            var val = db.StringGet(keyStr);
//            if (val.IsNullOrEmpty) {
//                if (db.LockTake("lock." + keyStr, "1", TimeSpan.FromSeconds(secord))) {
//                    val = db.StringGet(keyStr);
//                    if (val.IsNullOrEmpty) {
//                        var v = factory();
//                        var json = JsonConvert.SerializeObject(v);
//                        db.StringSet(keyStr, json, TimeSpan.FromSeconds(_timeout));
//                        db.LockRelease("lock." + keyStr, "1");
//                        return v;
//                    }
//                    db.LockRelease("lock." + keyStr, "1");
//                }
//            }
//            if (val.IsNullOrEmpty) { return default(TValue); }
//            var str = val.ToString();
//            return JsonConvert.DeserializeObject<TValue>(str);
//        }

//        /// <summary>
//        /// 执行
//        /// </summary>
//        /// <param name="key">值</param>
//        /// <param name="factory">执行方法</param>
//        /// <returns></returns>
//        public async Task<TValue> ExecuteAsync(TKey key, Func<Task<TValue>> factory)
//        {
//            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
//            IDatabase db = redis.GetDatabase(_databaseId);
//            var keyStr = _prefix + key.ToString();
//            var val = await db.StringGetAsync(keyStr);
//            if (val.IsNullOrEmpty) {
//                if (await db.LockTakeAsync("lock." + keyStr, "1", TimeSpan.FromSeconds(_timeout * 2))) {
//                    val = await db.StringGetAsync(keyStr);
//                    if (val.IsNullOrEmpty) {
//                        var v = await factory();
//                        var json = JsonConvert.SerializeObject(v);
//                        await db.StringSetAsync(keyStr, json, TimeSpan.FromSeconds(_timeout));
//                        await db.LockReleaseAsync("lock." + keyStr, "1");
//                        return v;
//                    }
//                    await db.LockReleaseAsync("lock." + keyStr, "1");
//                }
//            }
//            if (val.IsNullOrEmpty) { return default(TValue); }
//            var str = val.ToString();
//            return JsonConvert.DeserializeObject<TValue>(str);
//        }

//        /// <summary>
//        /// 执行
//        /// </summary>
//        /// <param name="key">值</param>
//        /// <param name="secord">每次超时秒数，最多8次</param>
//        /// <param name="factory">执行方法</param>
//        /// <returns></returns>
//        public async Task<TValue> ExecuteAsync(TKey key, int secord, Func<Task<TValue>> factory)
//        {
//            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
//            IDatabase db = redis.GetDatabase(_databaseId);
//            var keyStr = _prefix + key.ToString();
//            var val = await db.StringGetAsync(keyStr);
//            if (val.IsNullOrEmpty) {
//                if (await db.LockTakeAsync("lock." + keyStr, "1", TimeSpan.FromSeconds(_timeout * 2))) {
//                    val = await db.StringGetAsync(keyStr);
//                    if (val.IsNullOrEmpty) {
//                        var v = await factory();
//                        var json = JsonConvert.SerializeObject(v);
//                        await db.StringSetAsync(keyStr, json, TimeSpan.FromSeconds(_timeout));
//                        await db.LockReleaseAsync("lock." + keyStr, "1");
//                        return v;
//                    }
//                    await db.LockReleaseAsync("lock." + keyStr, "1");
//                }
//            }
//            if (val.IsNullOrEmpty) { return default(TValue); }
//            var str = val.ToString();
//            return JsonConvert.DeserializeObject<TValue>(str);
//        }

//        /// <summary>
//        /// 移除KEY
//        /// </summary>
//        /// <param name="key"></param>
//        public void Remove(TKey key)
//        {
//            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
//            IDatabase db = redis.GetDatabase(_databaseId);
//            var keyStr = _prefix + key.ToString();
//            db.KeyDelete(keyStr);
//        }

//    }
//}
