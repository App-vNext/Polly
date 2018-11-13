using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
    /// For example: 1s, 3s, 2s, 4s.
    /// For background and formula, see https://www.awsarchitectureblog.com/2015/03/backoff.html.
    /// </summary>
    public sealed class DecorrelatedJitterBackoff : ISleepDurationStrategy
    {
        private readonly Random _random;

        /// <summary>
        /// The maximum number of retries to use, in addition to the original call.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Whether the first retry will be immediate or not.
        /// </summary>
        public bool FastFirst { get; }

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
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        /// <param name="random">An optional <see cref="Random"/> instance to use.
        /// If not specified, will use a shared (singleton) instance with a random seed.</param>
        public DecorrelatedJitterBackoff(int retryCount, TimeSpan minDelay, TimeSpan maxDelay, bool fastFirst = false, Random random = null)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay));
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay));

            _random = random ?? DurationUtils.Random;

            RetryCount = retryCount;
            MinDelay = minDelay;
            MaxDelay = maxDelay;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        public IReadOnlyList<TimeSpan> Discrete()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];
            if (delays.Length == 0)
                return delays;

            int i = 0;
            if (FastFirst)
                delays[i++] = TimeSpan.Zero;

            double ms = MinDelay.TotalMilliseconds;
            for (; i < delays.Length; i++)
            {
                ms *= 3.0 * _random.NextDouble(); // [0.0, 3.0)
                ms = Math.Max(MinDelay.TotalMilliseconds, ms); // [min, N]
                ms = Math.Min(MaxDelay.TotalMilliseconds, ms); // [min, max]

                delays[i] = TimeSpan.FromMilliseconds(ms);
            }

            return delays;
        }

        /// <summary>
        /// Generate a continuous sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        public IEnumerable<TimeSpan> Take(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            int i = 0;
            if (FastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            double ms = MinDelay.TotalMilliseconds;
            double max = ms;

            for (; i < RetryCount && i < count; i++)
            {
                ms *= 3.0 * _random.NextDouble(); // [0.0, 3.0)
                ms = Math.Max(MinDelay.TotalMilliseconds, ms); // [min, N]
                ms = Math.Min(MaxDelay.TotalMilliseconds, ms); // [min, max]

                max = Math.Max(ms, max);

                yield return TimeSpan.FromMilliseconds(ms);
            }

            for (; i < count; i++)
            {
                yield return TimeSpan.FromMilliseconds(max);
            }
        }
    }
}
