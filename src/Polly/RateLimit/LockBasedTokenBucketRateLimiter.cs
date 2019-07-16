using System;
using Polly.Utilities;

namespace Polly.RateLimit
{
    /// <summary>
    /// A lock-based token-bucket rate-limiter for a Polly <see cref="IRateLimitPolicy"/>.
    /// </summary>
    internal class LockBasedTokenBucketRateLimiter : IRateLimiter
    {
        private readonly long addTokenTickInterval;
        private readonly long bucketCapacity;

        private long currentTokens;

        private long addNextTokenAtTicks;

        private readonly object _lock = new object();

        /// <summary>
        /// Creates an instance of <see cref="LockBasedTokenBucketRateLimiter"/>
        /// </summary>
        /// <param name="onePer">How often one execution is permitted.</param>
        /// <param name="bucketCapacity">The capacity of the token bucket.
        /// This equates to the maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
        /// </param>
        public LockBasedTokenBucketRateLimiter(TimeSpan onePer, long bucketCapacity)
        {
            if (onePer <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(onePer), $"The ${nameof(LockFreeTokenBucketRateLimiter)} must specify a positive TimeSpan for how often an execution is permitted.");
            if (bucketCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(bucketCapacity), $"The ${bucketCapacity} must be greater than or equal to 1.");

            addTokenTickInterval = onePer.Ticks;
            this.bucketCapacity = bucketCapacity;

            currentTokens = bucketCapacity;
            addNextTokenAtTicks = SystemClock.DateTimeOffsetUtcNow().Ticks + addTokenTickInterval;
        }

        /// <summary>
        /// Returns whether the execution is permitted; if not, returns what <see cref="TimeSpan"/> should be waited before retrying.
        /// </summary>
        public (bool permitExecution, TimeSpan retryAfter) PermitExecution()
        {
            using (TimedLock.Lock(_lock))
            {
                // Try to get a token.
                if (--currentTokens >= 0)
                {
                    // We got a token: permit execution!
                    return (true, TimeSpan.Zero);
                }
                else
                {
                    // No tokens! We're rate-limited - unless we can refill the bucket.
                    long now = SystemClock.DateTimeOffsetUtcNow().Ticks;

                    long ticksTillAddNextToken = addNextTokenAtTicks - now;
                    if (ticksTillAddNextToken > 0)
                    {
                        // Not time to add tokens yet: we're rate-limited!
                        return (false, TimeSpan.FromTicks(ticksTillAddNextToken));
                    }
                    else
                    {
                        // Time to add tokens to the bucket!

                        // We definitely need to add one token.  In fact, if we haven't hit this bit of code for a while, we might be due to add a bunch of tokens.
                        long tokensMissedAdding =
                            // Passing addNextTokenAtTicks merits one token
                            1 +
                            // And any whole token tick intervals further each merit another.
                            (-ticksTillAddNextToken / addTokenTickInterval);

                        // We mustn't exceed bucket capacity though. 
                        long tokensToAdd = Math.Min(bucketCapacity, tokensMissedAdding);

                        // Work out when tokens would next be due to be added, if we add these tokens.
                        long newAddNextTokenAtTicks = addNextTokenAtTicks + tokensToAdd * addTokenTickInterval;
                        // But if we were way overdue refilling the bucket (there was inactivity for a while), that value would be out-of-date: the next time we add tokens must be at least addTokenTickInterval from now.
                        newAddNextTokenAtTicks = Math.Max(newAddNextTokenAtTicks, now + addTokenTickInterval);

                        addNextTokenAtTicks = newAddNextTokenAtTicks;

                        // Theoretically we want to add tokensToAdd tokens.  But in fact we don't do that.  We want to claim one of those tokens for ourselves.
                        // So in fact we add (tokensToAdd - 1) tokens (ie we consume one), and return, permitting this execution.
                        currentTokens = tokensToAdd - 1;
                        return (true, TimeSpan.Zero);
                    }
                }
            }

        }
    }
}
