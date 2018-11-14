using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Generates sleep durations in an exponential manner.
    /// The formula used is: Duration = <see cref="Delay"/> x 2^iteration.
    /// For example: 1s, 2s, 4s, 8s.
    /// </summary>
    public sealed class ExponentialBackoff : ISleepDurationStrategy
    {
        /// <summary>
        /// Whether the first retry will be immediate or not.
        /// </summary>
        public bool FastFirst { get; }

        /// <summary>
        /// The duration value for the first retry.
        /// </summary>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="delay">The duration value for the first retry.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public ExponentialBackoff(TimeSpan delay, bool fastFirst = false)
        {
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            
            Delay = delay;
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

            double ms = Delay.TotalMilliseconds;
            for (; i < delays.Length; i++, ms *= 2.0)
            {
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

            double ms = Delay.TotalMilliseconds;
            double max = ms;

            for (; i < retryCount; i++, ms *= 2.0)
            {
                max = ms;
                yield return TimeSpan.FromMilliseconds(ms);
            }

            while (true)
            {
                yield return TimeSpan.FromMilliseconds(max);
            }
        }
    }
}
