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
        IReadOnlyList<TimeSpan> Generate();
    }
}
