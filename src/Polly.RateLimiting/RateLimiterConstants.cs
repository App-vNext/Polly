namespace Polly.RateLimiting;

internal static class RateLimiterConstants
{
    public const string DefaultName = "RateLimiter";

    public const int DefaultPermitLimit = 1000;

    public const int DefaultQueueLimit = 0;

    public const string OnRateLimiterRejectedEvent = "OnRateLimiterRejected";
}
