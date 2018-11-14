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
        private static readonly Random s_random = DurationUtils.Uniform;
        private readonly Random _random;

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
        /// <param name="minDelay">The minimum duration value to use for each retry.</param>
        /// <param name="maxDelay">The maximum duration value to use for each retry.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        /// <param name="random">An optional <see cref="Random"/> instance to use.
        /// If not specified, will use a shared (singleton) instance with a random seed.</param>
        public DecorrelatedJitterBackoff(TimeSpan minDelay, TimeSpan maxDelay, bool fastFirst = false, Random random = null)
        {
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay));
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay));

            _random = random ?? s_random;

            MinDelay = minDelay;
            MaxDelay = maxDelay;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        public IReadOnlyList<TimeSpan> Discrete(int retryCount)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));

            TimeSpan[] delays = new TimeSpan[retryCount];
            if (delays.Length == 0)
                return delays;

            int i = 0;
            if (FastFirst)
                delays[i++] = TimeSpan.Zero;

            double range = MaxDelay.TotalMilliseconds - MinDelay.Milliseconds;

            for (; i < delays.Length; i++)
            {
                double ms = range * _random.NextDouble(); // Range
                ms += MinDelay.TotalMilliseconds; // Floor
                ms = Math.Min(ms, MaxDelay.TotalMilliseconds); // Ceiling

                delays[i] = TimeSpan.FromMilliseconds(ms);
            }

            return delays;
        }

        /// <summary>
        /// Generate a continuous sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        public IEnumerable<TimeSpan> Continuous(int retryCount)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (retryCount == 0)
                yield break;

            int i = 0;
            if (FastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            double range = MaxDelay.TotalMilliseconds - MinDelay.Milliseconds;
            double max = MinDelay.TotalMilliseconds;

            for (; i < retryCount; i++)
            {
                double ms = range * _random.NextDouble(); // Range
                ms += MinDelay.TotalMilliseconds; // Floor
                ms = Math.Min(ms, MaxDelay.TotalMilliseconds); // Ceiling

                max = Math.Max(ms, max); // Extra

                yield return TimeSpan.FromMilliseconds(ms);
            }

            while (true)
            {
                yield return TimeSpan.FromMilliseconds(max);
            }
        }
    }
}
