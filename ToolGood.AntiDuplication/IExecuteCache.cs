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
    public interface IExecuteCache<TKey, TValue>
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        /// <returns></returns>
        TValue Execute(TKey key, Func<TValue> factory);


        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="secord">每次超时秒数，最多8次</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        TValue Execute(TKey key, int secord, Func<TValue> factory);

#if !NET40
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        /// <returns></returns>
        Task<TValue> ExecuteAsync(TKey key, Func<Task<TValue>> factory);

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="key">值</param>
        /// <param name="secord">每次超时秒数，最多8次</param>
        /// <param name="factory">执行方法</param>
        /// <returns></returns>
        Task<TValue> ExecuteAsync(TKey key, int secord, Func<Task<TValue>> factory); 
#endif


        /// <summary>
        /// 清空
        /// </summary>
        void Clear();

        /// <summary>
        /// 移除KEY
        /// </summary>
        /// <param name="key"></param>
        void Remove(TKey key);
    }
}
