#nullable enable
namespace Polly.RateLimit;

internal static class RateLimiterFactory
{
    public static IRateLimiter Create(TimeSpan onePer, int bucketCapacity) =>
        new LockFreeTokenBucketRateLimiter(onePer, bucketCapacity);
}
