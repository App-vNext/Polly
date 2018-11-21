using System;
using System.Collections.Generic;

namespace Polly.Retry
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
        IEnumerable<TimeSpan> Generate(int retryCount);
    }
}
