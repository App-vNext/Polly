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
        /// Whether the first retry will be immediate or not.
        /// </summary>
        bool FastFirst { get; }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        IReadOnlyList<TimeSpan> Discrete(int retryCount);

        /// <summary>
        /// Generate a continuous sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// Depending on the implementation, iterations higher than <paramref name="retryCount"/> may cap
        /// the value (using the last value) or continue producing values per normal.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="maxCount">The additional retries to return, using the maximum value of the previous phase.</param>
        IEnumerable<TimeSpan> Take(int retryCount, int maxCount);
    }
}
