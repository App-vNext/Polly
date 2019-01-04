using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
    /// For example: 117ms, 236ms, 141ms, 424ms, ...
    /// For background, see https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/.
    /// </summary>
    public static class DecorrelatedJitterBackoff_static // Temporary name
    {
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
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay), minDelay, "should be >= 0ms");
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay), maxDelay, $"should be >= {minDelay}");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");

            if (retryCount == 0)
                yield break;

            var random = new ConcurrentRandom(seed);

            int i = 0;
            if (fastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            double ms = minDelay.TotalMilliseconds;
            for (; i < retryCount; i++)
            {
                // https://github.com/aws-samples/aws-arch-backoff-simulator/blob/master/src/backoff_simulator.py#L45
                // self.sleep = min(self.cap, random.uniform(self.base, self.sleep * 3))

                // Formula avoids hard clamping (which empirically results in a bad distribution)
                double ceiling = Math.Min(maxDelay.TotalMilliseconds, ms * 3);
                ms = random.Uniform(minDelay.TotalMilliseconds, ceiling);

                yield return TimeSpan.FromMilliseconds(ms);
            }
        }
    }
}
