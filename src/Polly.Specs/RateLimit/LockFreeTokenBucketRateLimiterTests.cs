using System;
using Polly.RateLimit;

namespace Polly.Specs.RateLimit
{
    public class LockFreeTokenBucketRateLimiterTests : TokenBucketRateLimiterTestsBase
    {
        internal override IRateLimiter GetRateLimiter(TimeSpan onePer, long bucketCapacity)
            => new LockFreeTokenBucketRateLimiter(onePer, bucketCapacity);
    }
}
