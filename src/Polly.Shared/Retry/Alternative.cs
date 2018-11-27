using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// </summary>
    public static class Alternative // Experiment
    {
        /// <summary>
        /// Generates sleep durations as a constant value.
        /// The formula used is: Duration = <see cref="Delay"/>.
        /// For example: 200ms, 200ms, 200ms, ...
        /// <param name="delay">The constant wait duration before each retry.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> ConstantBackoff(TimeSpan delay, int retryCount, bool fastFirst = false)
        {
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay), delay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");

            if (retryCount == 0)
                yield break;

            int i = 0;
            if (fastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            for (; i < retryCount; i++)
            {
                yield return delay;
            }
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
        public static IEnumerable<TimeSpan> DecorrelatedJitterBackoff(TimeSpan minDelay, TimeSpan maxDelay, int retryCount, bool fastFirst = false, int? seed = null)
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

        /// <summary>
        /// Generates sleep durations in an exponential manner.
        /// The formula used is: Duration = <paramref name="initialDelay"/> x 2^iteration.
        /// For example: 100ms, 200ms, 400ms, 800ms, ...
        /// </summary>
        /// <param name="initialDelay">The duration value for the wait before the first retry.</param>
        /// <param name="factor">The exponent to multiply each subsequent duration by.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> ExponentialBackoff(TimeSpan initialDelay, int retryCount, double factor = 2.0, bool fastFirst = false)
        {
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), initialDelay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");
            if (factor < 1.0) throw new ArgumentOutOfRangeException(nameof(factor), factor, "should be >= 1.0");

            if (retryCount == 0)
                yield break;

            int i = 0;
            if (fastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            double ms = initialDelay.TotalMilliseconds;
            for (; i < retryCount; i++, ms *= factor)
            {
                yield return TimeSpan.FromMilliseconds(ms);
            }
        }

        /// <summary>
        /// Generates sleep durations in an linear manner.
        /// The formula used is: Duration = <paramref name="initialDelay"/> x (1 + <paramref name="factor"/> x iteration).
        /// For example: 100ms, 200ms, 300ms, 400ms, ...
        /// </summary>
        /// <param name="initialDelay">The duration value for the first retry.</param>
        /// <param name="factor">The linear factor to use for increasing the duration on subsequent calls.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> LinearBackoff(TimeSpan initialDelay, int retryCount, double factor = 1.0, bool fastFirst = false)
        {
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), initialDelay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");
            if (factor < 0) throw new ArgumentOutOfRangeException(nameof(factor), factor, "should be >= 0");

            if (retryCount == 0)
                yield break;

            int i = 0;
            if (fastFirst)
            {
                i++;
                yield return TimeSpan.Zero;
            }

            double ms = initialDelay.TotalMilliseconds;
            double ad = factor * ms;

            for (; i < retryCount; i++, ms += ad)
            {
                yield return TimeSpan.FromMilliseconds(ms);
            }
        }
    }
}
