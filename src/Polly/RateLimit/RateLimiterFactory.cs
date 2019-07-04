using System;

namespace Polly.RateLimit
{
    internal class RateLimiterFactory
    {
        public static IRateLimiter Create(TimeSpan onePer, int bucketCapacity)
            => new LockFreeTokenBucketRateLimiter(onePer, bucketCapacity);
/*
            => new LockBasedTokenBucketRateLimiter(onePer, bucketCapacity);
*/
    }
}
