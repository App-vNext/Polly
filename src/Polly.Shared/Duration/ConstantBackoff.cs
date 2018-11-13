using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations as a constant value.
    /// The formula used is: Duration = <see cref="Delay"/>.
    /// For example: 1s, 1s, 1s, 1s.
    /// </summary>
    public sealed class ConstantBackoff : ISleepDurationStrategy
    {
        /// <summary>
        /// The maximum number of retries to use, in addition to the original call.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// The duration value for the first retry.
        /// </summary>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="delay">The duration value for the first retry.</param>
        public ConstantBackoff(int retryCount, TimeSpan delay)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));

            RetryCount = retryCount;
            Delay = delay;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        public IReadOnlyList<TimeSpan> Generate()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];

            double ms = Delay.TotalMilliseconds;

            for (int i = 0; i < delays.Length; i++)
            {
                delays[i] = TimeSpan.FromMilliseconds(ms);
            }

            return delays;
        }
    }
}
