namespace Polly.Retry;

internal static class RetryConstants
{
    public const string StrategyType = "Retry";

    public const string OnRetryEvent = "OnRetry";

    public const RetryBackoffType DefaultBackoffType = RetryBackoffType.Constant;

    public const int DefaultRetryCount = 3;

    public const int MaxRetryCount = 100;

    public const int InfiniteRetryCount = -1;

    public static readonly TimeSpan DefaultBaseDelay = TimeSpan.FromSeconds(2);
}
