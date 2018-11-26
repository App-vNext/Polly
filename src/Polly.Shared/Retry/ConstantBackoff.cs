using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// Generates sleep durations as a constant value.
    /// The formula used is: Duration = <see cref="Delay"/>.
    /// For example: 200ms, 200ms, 200ms, ...
    /// </summary>
    public sealed class ConstantBackoff : ISleepDurationSeriesStrategy
    {
        /// <summary>
        /// Specifies whether the first retry will be immediate or not.
        /// </summary>
        /// <remarks>When true, a first retry is made after zero delay, after which the configured
        /// backoff strategy is followed. The overall number of retries is not increased.</remarks>
        public bool FastFirst { get; }

        /// <summary>
        /// The constant wait duration before each retry.
        /// </summary>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="delay">The constant wait duration before each retry.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public ConstantBackoff(TimeSpan delay, bool fastFirst = false)
        {
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));

            Delay = delay;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// For example: 200ms, 200ms, 200ms, ...
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

            for (; i < retryCount; i++)
            {
                yield return Delay;
            }
        }
    }
}
