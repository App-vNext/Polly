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
        /// Whether the first retry will be immediate or not.
        /// </summary>
        bool FastFirst { get; }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        IReadOnlyList<TimeSpan> Discrete();

        /// <summary>
        /// Generate a continuous sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// Depending on the implementation, iterations higher than <see cref="RetryCount"/> may cap
        /// the value (using the last value) or continue producing values per normal.
        /// </summary>
        IEnumerable<TimeSpan> Take(int count);
    }
}
