using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// Generates sleep durations in an linear manner.
    /// The formula used is: Duration = <see cref="Delay"/> + <see cref="Factor"/> x iteration x <see cref="Delay"/>.
    /// For example: 2s, 4s, 6s, 8s...
    /// </summary>
    public sealed class LinearBackoff : ISleepDurationStrategy
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
        /// The linear factor (gradient) to use for increasing the duration on subsequent calls.
        /// </summary>
        public double Factor { get; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="delay">The duration value for the first retry.</param>
        /// <param name="factor">The linear factor to use for increasing the duration on subsequent calls.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public LinearBackoff(TimeSpan delay, double factor = 1.0, bool fastFirst = false)
        {
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            if (factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));

            Delay = delay;
            Factor = factor;
            FastFirst = fastFirst;
        }

        /// <summary>
        /// Generate the sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// For example: 2s, 4s, 6s, 8s...
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

            double ms = Delay.TotalMilliseconds;
            double ad = Factor * ms;

            for (; i < retryCount; i++, ms += ad)
            {
                yield return TimeSpan.FromMilliseconds(ms);
            }
        }
    }
}
