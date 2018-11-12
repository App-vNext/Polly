using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
    /// For example: 1s, 3s, 2s, 4s.
    /// For background, see https://www.awsarchitectureblog.com/2015/03/backoff.html.
    /// </summary>
    public sealed class DecorrelatedJitterBackoff : ISleepDurationStrategy
    {
        private readonly Random _random;

        /// <summary>
        /// The maximum number of retries to use, in addition to the original call.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// The minimum duration value to use for each retry.
        /// </summary>
        public TimeSpan MinDelay { get; }

        /// <summary>
        /// The maximum duration value to use for each retry.
        /// </summary>
        public TimeSpan MaxDelay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="minDelay">The minimum duration value to use for each retry.</param>
        /// <param name="maxDelay">The maximum duration value to use for each retry.</param>
        /// <param name="random">An optional <see cref="Random"/> instance to use.
        /// If not specified, will use a shared (singleton) instance with a random seed.</param>
        public DecorrelatedJitterBackoff(int retryCount, TimeSpan minDelay, TimeSpan maxDelay, Random random = null)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay));
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay));

            _random = random ?? DurationUtils.Random;

            RetryCount = retryCount;
            MinDelay = minDelay;
            MaxDelay = maxDelay;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        public IReadOnlyList<TimeSpan> Generate()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];

            double ms = MinDelay.TotalMilliseconds;
            for (int i = 0; i < delays.Length; i++)
            {
                ms *= 3.0 * _random.NextDouble(); // [0.0, 3.0)
                ms = Math.Max(MinDelay.TotalMilliseconds, ms); // [min, N]
                ms = Math.Min(MaxDelay.TotalMilliseconds, ms); // [min, max]

                delays[i] = TimeSpan.FromMilliseconds(ms);
            }

            return delays;
        }
    }
}
