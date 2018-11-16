using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
    /// For example: 1s, 3s, 4s, 2s...
    /// For background, see https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/.
    /// </summary>
    public sealed class DecorrelatedJitterBackoff : ISleepDurationStrategy
    {
        private readonly ConcurrentRandom _random;

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
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed.</param>
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
        /// For example: 1s, 3s, 4s, 2s...
        /// </summary>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        public IEnumerable<TimeSpan> Generate(int retryCount)
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
                ms = _random.Uniform(MinDelay.TotalMilliseconds, ms * 3);
                ms = Math.Min(MaxDelay.TotalMilliseconds, ms);

                yield return TimeSpan.FromMilliseconds(ms);
            }
        }

        #region Nested

        /// <summary>
        /// A random number generator with a Uniform distribution that is thread-safe (via locking).
        /// Can be instantiated with a custom <see cref="Random"/> instance, for example to make
        /// it act in a deterministic manner.
        /// </summary>
        private sealed class ConcurrentRandom
        {
            /// <summary>
            /// A shared instance of <see cref="Random"/> that is safe to use concurrently.
            /// </summary>
            private static readonly Random s_random = new Random();

            private readonly Random _random;

            /// <summary>
            /// Creates an instance of the <see cref="ConcurrentRandom"/> class.
            /// </summary>
            /// <param name="seed">An optional <see cref="Random"/> seed to use.
            /// If not specified, will use a shared instance with a random seed.</param>
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

                // In order to match AWS guidance, this closely follows python docs:
                // https://docs.python.org/2/library/random.html#random.uniform
                return a + (b - a) * NextDouble();
            }
        }

        #endregion
    }
}
