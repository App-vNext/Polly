using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
    /// For example: 117ms, 236ms, 141ms, 424ms, ...
    /// For background, see https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/.
    /// </summary>
    public sealed class DecorrelatedJitterBackoff : ISleepDurationSeriesStrategy
    {
        private readonly ConcurrentRandom _random;

        /// <summary>
        /// Specifies whether the first retry will be immediate or not.
        /// </summary>
        /// <remarks>When true, a first retry is made after zero delay, after which the configured
        /// backoff strategy is followed. The overall number of retries is not increased.</remarks>
        public bool FastFirst { get; }

        /// <summary>
        /// The minimum duration value to use for the wait before each retry.
        /// </summary>
        public TimeSpan MinDelay { get; }

        /// <summary>
        /// The maximum duration value to use for the wait before each retry.
        /// </summary>
        public TimeSpan MaxDelay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="minDelay">The minimum duration value to use for the wait before each retry.</param>
        /// <param name="maxDelay">The maximum duration value to use for the wait before each retry.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
        public DecorrelatedJitterBackoff(TimeSpan minDelay, TimeSpan maxDelay, bool fastFirst = false, int? seed = null)
        {
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay), minDelay, "should be >= 0ms");
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay), maxDelay, $"should be >= {minDelay}");

            _random = new ConcurrentRandom(seed);

            MinDelay = minDelay;
            MaxDelay = maxDelay;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
        /// For example: 117ms, 236ms, 141ms, 424ms, ...
        /// For background, see https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/.
        /// </summary>
        /// <param name="minDelay">The minimum duration value to use for the wait before each retry.</param>
        /// <param name="maxDelay">The maximum duration value to use for the wait before each retry.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
        public static IEnumerable<TimeSpan> Create(TimeSpan minDelay, TimeSpan maxDelay, int retryCount, bool fastFirst = false, int? seed = null)
        {
            var backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, seed);
            return backoff.GetSleepDurations(retryCount);
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
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

            double ms = MinDelay.TotalMilliseconds;
            for (; i < retryCount; i++)
            {
                // https://github.com/aws-samples/aws-arch-backoff-simulator/blob/master/src/backoff_simulator.py#L45
                // self.sleep = min(self.cap, random.uniform(self.base, self.sleep * 3))

                // Formula avoids hard clamping (which empirically results in a bad distribution)
                double ceiling = Math.Min(MaxDelay.TotalMilliseconds, ms * 3);
                ms = _random.Uniform(MinDelay.TotalMilliseconds, ceiling);

                yield return TimeSpan.FromMilliseconds(ms);
            }
        }
    }
}
