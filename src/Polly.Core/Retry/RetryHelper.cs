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
#if !NETCOREAPP
            RetryBackoffType.Linear => TimeSpan.FromMilliseconds((attempt + 1) * baseDelay.TotalMilliseconds),
            RetryBackoffType.Exponential => TimeSpan.FromMilliseconds(Math.Pow(ExponentialFactor, attempt) * baseDelay.TotalMilliseconds),
#else
            RetryBackoffType.Linear => (attempt + 1) * baseDelay,
            RetryBackoffType.Exponential => Math.Pow(ExponentialFactor, attempt) * baseDelay,
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "The retry backoff type is not supported.")
        };
    }
}
