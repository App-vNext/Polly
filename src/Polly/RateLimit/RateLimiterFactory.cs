using System;

namespace Polly.RateLimit
{
    internal static class RateLimiterFactory
    {
        public static IRateLimiter Create(TimeSpan onePer, int bucketCapacity)
            => new LockFreeTokenBucketRateLimiter(onePer, bucketCapacity);

        public static IRateLimiter CreateSlidingWindowRateLimiter(TimeSpan perTimeSpan, int numberOfExecutions)
            => new SlidingWindowRateLimiter(perTimeSpan, numberOfExecutions);
    }
}
