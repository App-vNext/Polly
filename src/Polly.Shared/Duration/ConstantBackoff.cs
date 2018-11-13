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
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="delay">The duration value for the first retry.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public ConstantBackoff(int retryCount, TimeSpan delay, bool fastFirst = false)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));

            RetryCount = retryCount;
            Delay = delay;
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

            for (; i < delays.Length; i++)
            {
                delays[i] = Delay;
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

            for (; i < count; i++)
            {
                yield return Delay;
            }
        }
    }
}
