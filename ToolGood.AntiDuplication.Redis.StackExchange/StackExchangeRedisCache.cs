using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace ToolGood.AntiDuplication.Redis.StackExchange
{
    public class StackExchangeRedisCache<TKey, TValue> : IExecuteCache<TKey, TValue>
    {
        private readonly string _rediesConnStr;
        private readonly int _databaseId;
        private readonly string _prefix;
        private readonly int _timeout;


        public StackExchangeRedisCache(string rediesConnStr, int databaseId, string prefix, int timeout)
        {
            _rediesConnStr = rediesConnStr;
            _databaseId = databaseId;
            _prefix = prefix;
            _timeout = timeout;
        }


        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        public TValue Execute(TKey key, Func<TValue> factory)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
            IDatabase db = redis.GetDatabase(_databaseId);
            var keyStr = _prefix + key.ToString();
            var val = db.StringGet(keyStr);
            if (val.IsNullOrEmpty) {
                if (db.LockTake("lock." + keyStr, "1", TimeSpan.FromSeconds(_timeout * 2))) {
                    val = db.StringGet(keyStr);
                    if (val.IsNullOrEmpty) {
                        var v = factory();
                        var json = JsonConvert.SerializeObject(v);
                        db.StringSet(keyStr, json, TimeSpan.FromSeconds(_timeout));
                        db.LockRelease("lock." + keyStr, "1");
                        return v;
                    }
                    db.LockRelease("lock." + keyStr, "1");
                }
            }
            if (val.IsNullOrEmpty) { return default(TValue); }
            var str = val.ToString();
            return JsonConvert.DeserializeObject<TValue>(str);
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
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
            IDatabase db = redis.GetDatabase(_databaseId);
            var keyStr = _prefix + key.ToString();
            var val = db.StringGet(keyStr);
            if (val.IsNullOrEmpty) {
                if (db.LockTake("lock." + keyStr, "1", TimeSpan.FromSeconds(secord))) {
                    val = db.StringGet(keyStr);
                    if (val.IsNullOrEmpty) {
                        var v = factory();
                        var json = JsonConvert.SerializeObject(v);
                        db.StringSet(keyStr, json, TimeSpan.FromSeconds(_timeout));
                        db.LockRelease("lock." + keyStr, "1");
                        return v;
                    }
                    db.LockRelease("lock." + keyStr, "1");
                }
            }
            if (val.IsNullOrEmpty) { return default(TValue); }
            var str = val.ToString();
            return JsonConvert.DeserializeObject<TValue>(str);
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        public async Task<TValue> ExecuteAsync(TKey key, Func<Task<TValue>> factory)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
            IDatabase db = redis.GetDatabase(_databaseId);
            var keyStr = _prefix + key.ToString();
            var val = await db.StringGetAsync(keyStr);
            if (val.IsNullOrEmpty) {
                if (await db.LockTakeAsync("lock." + keyStr, "1", TimeSpan.FromSeconds(_timeout * 2))) {
                    val = await db.StringGetAsync(keyStr);
                    if (val.IsNullOrEmpty) {
                        var v = await factory();
                        var json = JsonConvert.SerializeObject(v);
                        await db.StringSetAsync(keyStr, json, TimeSpan.FromSeconds(_timeout));
                        await db.LockReleaseAsync("lock." + keyStr, "1");
                        return v;
                    }
                    await db.LockReleaseAsync("lock." + keyStr, "1");
                }
            }
            if (val.IsNullOrEmpty) { return default(TValue); }
            var str = val.ToString();
            return JsonConvert.DeserializeObject<TValue>(str);
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="secord">每次超时秒数，最多8次</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        public async Task<TValue> ExecuteAsync(TKey key, int secord, Func<Task<TValue>> factory)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
            IDatabase db = redis.GetDatabase(_databaseId);
            var keyStr = _prefix + key.ToString();
            var val = await db.StringGetAsync(keyStr);
            if (val.IsNullOrEmpty) {
                if (await db.LockTakeAsync("lock." + keyStr, "1", TimeSpan.FromSeconds(_timeout * 2))) {
                    val = await db.StringGetAsync(keyStr);
                    if (val.IsNullOrEmpty) {
                        var v = await factory();
                        var json = JsonConvert.SerializeObject(v);
                        await db.StringSetAsync(keyStr, json, TimeSpan.FromSeconds(_timeout));
                        await db.LockReleaseAsync("lock." + keyStr, "1");
                        return v;
                    }
                    await db.LockReleaseAsync("lock." + keyStr, "1");
                }
            }
            if (val.IsNullOrEmpty) { return default(TValue); }
            var str = val.ToString();
            return JsonConvert.DeserializeObject<TValue>(str);
        }

        /// <summary>
        /// 移除KEY
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_rediesConnStr);
            IDatabase db = redis.GetDatabase(_databaseId);
            var keyStr = _prefix + key.ToString();
            db.KeyDelete(keyStr);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
        }




    }
}
