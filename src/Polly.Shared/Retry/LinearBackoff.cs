using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// Generates sleep durations in an linear manner.
    /// The formula used is: Duration = <see cref="InitialDelay"/> x (1 + <see cref="Factor"/> x iteration).
    /// For example: 100ms, 200ms, 300ms, 400ms, ...
    /// </summary>
    public sealed class LinearBackoff : ISleepDurationSeriesStrategy
    {
        /// <summary>
        /// Specifies whether the first retry will be immediate or not.
        /// </summary>
        /// <remarks>When true, a first retry is made after zero delay, after which the configured
        /// backoff strategy is followed. The overall number of retries is not increased.</remarks>
        public bool FastFirst { get; }

        /// <summary>
        /// The duration value for the wait before the first retry.
        /// </summary>
        public TimeSpan InitialDelay { get; }

        /// <summary>
        /// The linear factor (gradient) to use for increasing the duration on subsequent calls.
        /// </summary>
        public double Factor { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="initialDelay">The duration value for the first retry.</param>
        /// <param name="factor">The linear factor to use for increasing the duration on subsequent calls.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public LinearBackoff(TimeSpan initialDelay, double factor = 1.0, bool fastFirst = false)
        {
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay));
            if (factor < 0) throw new ArgumentOutOfRangeException(nameof(factor));

            InitialDelay = initialDelay;
            Factor = factor;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// For example: 100ms, 200ms, 300ms, 400ms, ...
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

            double ms = InitialDelay.TotalMilliseconds;
            double ad = Factor * ms;

            for (; i < retryCount; i++, ms += ad)
            {
                yield return TimeSpan.FromMilliseconds(ms);
            }
        }
    }
}
