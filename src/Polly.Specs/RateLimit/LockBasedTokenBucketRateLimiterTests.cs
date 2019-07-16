using System;
using Polly.RateLimit;

namespace Polly.Specs.RateLimit
{
    public class LockBasedTokenBucketRateLimiterTests : TokenBucketRateLimiterTestsBase
    {
        internal override IRateLimiter GetRateLimiter(TimeSpan onePer, long bucketCapacity)
            => new LockBasedTokenBucketRateLimiter(onePer, bucketCapacity);
    }
}
