using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// 清空
        /// </summary>
        void Clear();
    }
}
