using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations in an exponential manner.
    /// For example: 1s, 2s, 4s, 8s.
    /// </summary>
    public sealed class ExponentialBackoff : ISleepDurationStrategy
    {
        /// <summary>
        /// The maximum number of retries to use, in addition to the original call.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// The duration value for the first retry.
        /// </summary>
        public TimeSpan InitialDelay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="initialDelay">The duration value for the first retry.</param>
        public ExponentialBackoff(int retryCount, TimeSpan initialDelay)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay));

            RetryCount = retryCount;
            InitialDelay = initialDelay;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<TimeSpan> Generate()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];

            double ms = InitialDelay.TotalMilliseconds;
            for (int i = 0; i < delays.Length; i++)
            {
                delays[i] = TimeSpan.FromMilliseconds(ms);

                ms *= 2.0;
            }

            return delays;
        }
    }
}
