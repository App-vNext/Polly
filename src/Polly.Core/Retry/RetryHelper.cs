using System;

namespace Polly.Retry;

internal static class RetryHelper
{
    private const double ExponentialFactor = 2.0;

    public static bool IsValidDelay(TimeSpan delay) => delay >= TimeSpan.Zero;

    public static TimeSpan GetRetryDelay(RetryBackoffType type, int attempt, TimeSpan baseDelay)
    {
        if (baseDelay == TimeSpan.Zero)
        {
            return baseDelay;
        }

        return type switch
        {
            RetryBackoffType.Constant => baseDelay,
            RetryBackoffType.Linear => TimeSpan.FromMilliseconds((attempt + 1) * baseDelay.TotalMilliseconds),
            RetryBackoffType.Exponential => TimeSpan.FromMilliseconds(Math.Pow(ExponentialFactor, attempt) * baseDelay.TotalMilliseconds),
            _ => throw new NotSupportedException()
        };
    }
}
