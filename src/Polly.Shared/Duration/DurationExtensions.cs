using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// Extensions related to <see cref="ISleepDurationStrategy"/>.
    /// </summary>
    public static class DurationExtensions
    {
        /// <summary>
        /// Generate a continuous sequence of <see cref="TimeSpan"/> values to use as sleep-durations.
        /// The first <paramref name="retryCount"/> durations are generated in the same manner as
        /// the corresponding <see cref="ISleepDurationStrategy"/>, and thereafter the maximum value 
        /// from that sequence is returned.
        /// For example: 2s, 4s, 6s, 8s; 8s, 8s, 8s...
        /// </summary>
        /// <param name="strategy">The duration strategy to use.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        public static IEnumerable<TimeSpan> Continuous(this ISleepDurationStrategy strategy, int retryCount)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));

            double max = 0;
            foreach (TimeSpan delay in strategy.Generate(retryCount))
            {
                max = Math.Max(max, delay.TotalMilliseconds);

                yield return delay;
            }

            while (true)
            {
                yield return TimeSpan.FromMilliseconds(max);
            }
        }
    }
}
