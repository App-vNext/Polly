using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay));
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay));

            _random = new ConcurrentRandom(seed);

            MinDelay = minDelay;
            MaxDelay = maxDelay;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// For example: 117ms, 236ms, 141ms, 424ms, ...
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        public IEnumerable<TimeSpan> GetSleepDurations(int retryCount)
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

        #region Nested

        /// <summary>
        /// A random number generator with a Uniform distribution that is thread-safe (via locking).
        /// Can be instantiated with a custom <see cref="int"/> seed, for example to make
        /// it act in a deterministic manner.
        /// </summary>
        private sealed class ConcurrentRandom
        {
            // Singleton approach is per MS best-practices.
            // https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netframework-4.7.2#the-systemrandom-class-and-thread-safety
            // https://stackoverflow.com/a/25448166/
            // Also note that in concurrency testing, using a 'new Random()' for every thread ended up
            // being highly correlated. On NetFx this is maybe due to the same seed somehow being used 
            // in each instance, but either way the singleton approach mitigated the problem.
            private static readonly Random s_random = new Random();
            private readonly Random _random;

            /// <summary>
            /// Creates an instance of the <see cref="ConcurrentRandom"/> class.
            /// </summary>
            /// <param name="seed">An optional <see cref="Random"/> seed to use.
            /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
            public ConcurrentRandom(int? seed = null)
            {
                _random = seed == null
                    ? s_random // Do not use 'new Random()' here; in concurrent scenarios they could have the same seed
                    : new Random(seed.Value);
            }

            /// <summary>
            /// Returns a random floating-point number that is greater than or equal to 0.0,
            /// and less than 1.0.
            /// This method uses locks in order to avoid issues with concurrent access.
            /// </summary>
            public double NextDouble()
            {
                // It is safe to lock on _random since it's not exposed
                // to outside use so it cannot be contended.
                lock (_random)
                {
                    return _random.NextDouble();
                }
            }

            /// <summary>
            /// Returns a random floating-point number that is greater than or equal to <paramref name="a"/>,
            /// and less than <paramref name="b"/>.
            /// </summary>
            /// <param name="a">The minimum value.</param>
            /// <param name="b">The maximum value.</param>
            public double Uniform(double a, double b)
            {
                Debug.Assert(a <= b);

                if (a == b) return a;

                return a + (b - a) * NextDouble();
            }
        }

        #endregion
    }
}
