﻿#nullable enable

namespace Polly.RateLimit;

/// <summary>
/// A lock-free token-bucket rate-limiter for a Polly <see cref="IRateLimitPolicy"/>.
/// </summary>
internal sealed class LockFreeTokenBucketRateLimiter : IRateLimiter
{
    private readonly long _addTokenTickInterval;
    private readonly long _bucketCapacity;

    private long _currentTokens;

    private long _addNextTokenAtTicks;

#if !NETSTANDARD2_0
    private SpinWait _spinner = default;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreeTokenBucketRateLimiter"/> class.
    /// </summary>
    /// <param name="onePer">How often one execution is permitted.</param>
    /// <param name="bucketCapacity">The capacity of the token bucket.
    /// This equates to the maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
    /// </param>
    public LockFreeTokenBucketRateLimiter(TimeSpan onePer, long bucketCapacity)
    {
        if (onePer <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(onePer), onePer, $"The {nameof(LockFreeTokenBucketRateLimiter)} must specify a positive TimeSpan for how often an execution is permitted.");
        }

        if (bucketCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bucketCapacity), bucketCapacity, $"{nameof(bucketCapacity)} must be greater than or equal to 1.");
        }

        _addTokenTickInterval = onePer.Ticks;
        _bucketCapacity = bucketCapacity;

        _currentTokens = bucketCapacity;
        _addNextTokenAtTicks = SystemClock.DateTimeOffsetUtcNow().Ticks + _addTokenTickInterval;
    }

    /// <summary>
    /// Returns whether the execution is permitted; if not, returns what <see cref="TimeSpan"/> should be waited before retrying.
    /// </summary>
    public (bool PermitExecution, TimeSpan RetryAfter) PermitExecution()
    {
        while (true)
        {
            // Try to get a token.
            long tokensAfterGrabOne = Interlocked.Decrement(ref _currentTokens);

            if (tokensAfterGrabOne >= 0)
            {
                // We got a token: permit execution!
                return (true, TimeSpan.Zero);
            }

            // No tokens! We're rate-limited - unless we can refill the bucket.
            long now = SystemClock.DateTimeOffsetUtcNow().Ticks;
            long currentAddNextTokenAtTicks = Interlocked.Read(ref _addNextTokenAtTicks);
            long ticksTillAddNextToken = currentAddNextTokenAtTicks - now;

            if (ticksTillAddNextToken > 0)
            {
                // Not time to add tokens yet: we're rate-limited!
                return (false, TimeSpan.FromTicks(ticksTillAddNextToken));
            }

            // Time to add tokens to the bucket!

            // We definitely need to add one token. In fact, if we haven't hit this bit of code for a while, we might be due to add a bunch of tokens.
            // Passing addNextTokenAtTicks merits one token and any whole token tick intervals further each merit another.
            long tokensMissedAdding = 1 + (-ticksTillAddNextToken / _addTokenTickInterval);

            // We mustn't exceed bucket capacity though.
            long tokensToAdd = Math.Min(_bucketCapacity, tokensMissedAdding);

            // Work out when tokens would next be due to be added, if we add these tokens.
            long newAddNextTokenAtTicks = currentAddNextTokenAtTicks + (tokensToAdd * _addTokenTickInterval);

            // But if we were way overdue refilling the bucket (there was inactivity for a while), that value would be out-of-date: the next time we add tokens must be at least addTokenTickInterval from now.
            newAddNextTokenAtTicks = Math.Max(newAddNextTokenAtTicks, now + _addTokenTickInterval);

            // Now see if we win the race to add these tokens.  Other threads might be racing through this code at the same time: only one thread must add the tokens!
            if (Interlocked.CompareExchange(ref _addNextTokenAtTicks, newAddNextTokenAtTicks, currentAddNextTokenAtTicks) == currentAddNextTokenAtTicks)
            {
                // We won the race to add the tokens!

                // Theoretically we want to add tokensToAdd tokens.  But in fact we don't do that.
                // We want to claim one of those tokens for ourselves - there's no way we're going to add it but let another thread snatch it from under our nose.
                // (Doing that could leave this thread looping round adding tokens for ever which other threads just snatch - would lead to odd observed behaviour.)

                // So in fact we add (tokensToAdd - 1) tokens (ie we consume one), and return, permitting this execution.

                // The advantage of only adding tokens when the bucket is empty is that we can now hard set the new amount of tokens (Interlocked.Exchange) without caring if other threads have simultaneously been taking or adding tokens.
                // (If we added a token per addTokenTickInterval to a non-empty bucket, the reasoning about not overflowing the bucket seems harder.)
                Interlocked.Exchange(ref _currentTokens, tokensToAdd - 1);
                return (true, TimeSpan.Zero);
            }
            else
            {
                // We didn't win the race to add the tokens. BUT because it _was_ time to add tokens, another thread must have won that race and have added/be adding tokens, so there _may_ be more tokens, so loop and try again.

                // We want any thread refilling the bucket to have a chance to do so before we try to grab the next token.
#if NETSTANDARD2_0
                Thread.Sleep(0);
#else
                _spinner.SpinOnce();
#endif
            }
        }
    }
}
