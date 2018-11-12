using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LinearBackoff : ISleepDurationStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan MinDelay { get; }

        /// <summary>
        /// 
        /// </summary>
        public double Factor { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="minDelay"></param>
        /// <param name="factor"></param>
        public LinearBackoff(int retryCount, TimeSpan minDelay, double factor = 1.0)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay));
            if (factor == 0) throw new ArgumentOutOfRangeException(nameof(factor));

            RetryCount = retryCount;
            MinDelay = minDelay;
            Factor = factor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<TimeSpan> Generate()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];

            double ms = MinDelay.TotalMilliseconds;
            double ad = Factor * MinDelay.TotalMilliseconds;

            for (int i = 0; i < delays.Length; i++)
            {
                ms += ad;

                delays[i] = TimeSpan.FromMilliseconds(ms);
            }

            return delays;
        }
    }
}
