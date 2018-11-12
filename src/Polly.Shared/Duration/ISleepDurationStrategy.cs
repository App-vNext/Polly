using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// An interface defining strategies for managing the sleep-duration of retries. 
    /// </summary>
    public interface ISleepDurationStrategy
    {
        /// <summary>
        /// The maximum number of retries to use, in addition to the original call.
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<TimeSpan> Generate();
    }
}
