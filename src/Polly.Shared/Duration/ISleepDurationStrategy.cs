using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISleepDurationStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<TimeSpan> Create(Context content = null);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //TimeSpan Next(int i);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //TimeSpan Next2(int i, Context context);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TimeSpan Next<TResult>(int i, Context context, DelegateResult<TResult> @delegate);
    }
}
