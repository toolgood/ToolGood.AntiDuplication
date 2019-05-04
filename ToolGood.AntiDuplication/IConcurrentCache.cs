using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolGood.AntiDuplication
{
    /// <summary>
    /// 执行缓存接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IConcurrentCache<TKey, TValue>
    {

        /// <summary>
        /// 获取缓存个数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 是否为空
        /// </summary>
        bool IsEmpty { get; }

        #region TValue

        /// <summary>
        /// 获取或添加缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="secord">每次超时秒数</param>
        /// <param name="val">值</param>
        /// <returns></returns>
        TValue GetOrAdd(TKey key, TValue val, int secord = 0);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="secord">每次超时秒数</param>
        /// <param name="value">值</param>
        void SetValue(TKey key, TValue value, int secord = 0);

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="addValue">添加值</param>
        /// <param name="updateValue">更新值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue, int secord = 0);

        /// <summary>
        /// 尝试获取缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="secord">每次超时秒数</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool TryGetValue(TKey key, out TValue value, int secord = 0);

        /// <summary>
        /// 尝试添加缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryAdd(TKey key, TValue value, int secord = 0);
        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="newValue">新值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryUpdate(TKey key, TValue newValue, int secord = 0);

        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="newValue">新值</param>
        /// <param name="comparisonValue">原值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue, int secord = 0);

        /// <summary>
        /// 尝试删除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryRemove(TKey key, out TValue value, int secord = 0);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="secord">每次超时秒数</param>
        void Remove(TKey key, int secord = 0);

        /// <summary>
        /// 是否包含缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secord"></param>
        /// <returns></returns>
        bool ContainsKey(TKey key, int secord = 0);

        #endregion

        #region Func<TValue> factory
        /// <summary>
        /// 获取或添加缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="secord">每次超时秒数</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        TValue GetOrAdd(TKey key, Func<TValue> factory, int secord = 0);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="factory">执行方法</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        TValue SetValue(TKey key, Func<TValue> factory, int secord = 0);

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="addValueFactory">添加的值</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        TValue AddOrUpdate(TKey key, Func<TValue> addValueFactory, Func<TValue> updateValueFactory, int secord);

        /// <summary>
        /// 尝试添加缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="factory"></param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryAdd(TKey key, Func<TValue> factory, int secord = 0);
        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryUpdate(TKey key, Func<TValue> updateValueFactory, int secord = 0);

        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="comparisonValue">原值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        bool TryUpdate(TKey key, Func<TValue> updateValueFactory, TValue comparisonValue, int secord = 0);

        #endregion

        #region  Func<TValue> factory
#if !NET40
        /// <summary>
        /// 获取或添加缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="secord">每次超时秒数</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        Task<TValue> GetOrAdd(TKey key, Func<Task<TValue>> factory, int secord = 0);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="factory">执行方法</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        Task<TValue> SetValue(TKey key, Func<Task<TValue>> factory, int secord = 0);

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="addValueFactory">添加的值</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        Task<TValue> AddOrUpdate(TKey key, Func<Task<TValue>> addValueFactory, Func<Task<TValue>> updateValueFactory, int secord);

        /// <summary>
        /// 尝试添加缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="factory"></param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        Task<bool> TryAdd(TKey key, Func<Task<TValue>> factory, int secord = 0);
        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        Task<bool> TryUpdate(TKey key, Func<Task<TValue>> updateValueFactory, int secord = 0);
        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="comparisonValue">原值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        Task<bool> TryUpdate(TKey key, Func<Task<TValue>> updateValueFactory, TValue comparisonValue, int secord = 0);
        /// <summary>
        /// 尝试更新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="updateValueFactory">更新的值</param>
        /// <param name="comparisonValueFactory">原值</param>
        /// <param name="secord">每次超时秒数</param>
        /// <returns></returns>
        Task<bool> TryUpdate(TKey key, Func<Task<TValue>> updateValueFactory, Func<Task<TValue>> comparisonValueFactory, int secord = 0);

#endif
        #endregion

        /// <summary>
        /// 清空
        /// </summary>
        void Clear();

    }
}
