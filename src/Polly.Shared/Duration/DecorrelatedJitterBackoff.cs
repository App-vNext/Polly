using System;
using System.Collections.Generic;

namespace Polly.Duration
{
    // Background: https://www.awsarchitectureblog.com/2015/03/backoff.html

    /// <summary>
    /// 
    /// </summary>
    public sealed class DecorrelatedJitterBackoff : ISleepDurationStrategy
    {
        private readonly Random _random;

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
        public TimeSpan MaxDelay { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="minDelay"></param>
        /// <param name="maxDelay"></param>
        /// <param name="random"></param>
        public DecorrelatedJitterBackoff(int retryCount, TimeSpan minDelay, TimeSpan maxDelay, Random random = null)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay));
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay));

            _random = random ?? Jitter.Random;

            RetryCount = retryCount;
            MinDelay = minDelay;
            MaxDelay = maxDelay;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<TimeSpan> Generate()
        {
            TimeSpan[] delays = new TimeSpan[RetryCount];

            double ms = MinDelay.TotalMilliseconds;
            for (int i = 0; i < delays.Length; i++)
            {
                ms *= 3.0 * _random.NextDouble(); // [0.0, 3.0)
                ms = Math.Max(MinDelay.TotalMilliseconds, ms); // [min, N]
                ms = Math.Min(MaxDelay.TotalMilliseconds, ms); // [min, max]

                delays[i] = TimeSpan.FromMilliseconds(ms);
            }

            return delays;
        }
    }
}
