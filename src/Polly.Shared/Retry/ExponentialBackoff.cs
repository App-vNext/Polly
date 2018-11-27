using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// Generates sleep durations in an exponential manner.
    /// The formula used is: Duration = <see cref="InitialDelay"/> x 2^iteration.
    /// For example: 100ms, 200ms, 400ms, 800ms, ...
    /// </summary>
    public sealed class ExponentialBackoff : ISleepDurationSeriesStrategy
    {
        /// <summary>
        /// Specifies whether the first retry will be immediate or not.
        /// </summary>
        /// <remarks>When true, a first retry is made after zero delay, after which the configured
        /// backoff strategy is followed. The overall number of retries is not increased.</remarks>
        public bool FastFirst { get; }

        /// <summary>
        /// The duration value for the wait before the first retry.
        /// </summary>
        public TimeSpan InitialDelay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="initialDelay">The duration value for the wait before the first retry.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public ExponentialBackoff(TimeSpan initialDelay, bool fastFirst = false)
        {
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), initialDelay, "should be >= 0ms");

            InitialDelay = initialDelay;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// For example: 100ms, 200ms, 400ms, 800ms, ...
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        public IEnumerable<TimeSpan> GetSleepDurations(int retryCount)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");

            if (retryCount == 0)
                yield break;

            int i = 0;
            if (FastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            double ms = InitialDelay.TotalMilliseconds;
            for (; i < retryCount; i++, ms *= 2.0)
            {
                yield return TimeSpan.FromMilliseconds(ms);
            }
        }
    }
}
