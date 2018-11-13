using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations in an linear manner.
    /// The formula used is: Duration = <see cref="Factor"/> x <see cref="RetryCount"/> + <see cref="InitialDelay"/>.
    /// For example: 1s, 2s, 3s, 4s.
    /// </summary>
    public sealed class LinearBackoff : ISleepDurationStrategy
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
        /// The linear factor to use for increasing the delay on subsequent calls.
        /// </summary>
        public double Factor { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="initialDelay">The duration value for the first retry.</param>
        /// <param name="factor">The linear factor to use for increasing the duration on subsequent calls.</param>
        public LinearBackoff(int retryCount, TimeSpan initialDelay, double factor = 1.0)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay));
            if (factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));

            RetryCount = retryCount;
            InitialDelay = initialDelay;
            Factor = factor;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        public IReadOnlyList<TimeSpan> Generate()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];

            double ms = InitialDelay.TotalMilliseconds;
            double ad = Factor * InitialDelay.TotalMilliseconds;

            for (int i = 0; i < delays.Length; i++)
            {
                delays[i] = TimeSpan.FromMilliseconds(ms);

                ms += ad;
            }

            return delays;
        }
    }
}
