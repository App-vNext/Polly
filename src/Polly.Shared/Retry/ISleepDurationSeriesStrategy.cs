using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// An interface defining strategies for managing the sleep-duration of retries. 
    /// </summary>
    public interface ISleepDurationSeriesStrategy
    {
        /// <summary>
        /// Specifies whether the first retry will be immediate or not.
        /// </summary>
        /// <remarks>When true, a first retry is made after zero delay, after which the configured
        /// backoff strategy is followed. The overall number of retries is not increased.</remarks>
        bool FastFirst { get; }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        IEnumerable<TimeSpan> GetSleepDurations(int retryCount);
    }
}
