namespace Polly.Retry;

internal static class RetryConstants
{
    public const string StrategyType = "Retry";

    public const string OnRetryEvent = "OnRetry";

    public const RetryBackoffType DefaultBackoffType = RetryBackoffType.Exponential;

    public const int DefaultRetryCount = 3;

    /// <summary>
    /// Maximal allowed retry counts unless infinite.
    /// </summary>
    public const int MaxRetryCount = 100;

    /// <summary>
    /// Maximal allowed BaseDelay (1 day).
    /// </summary>
    public const int MaxBaseDelay = 24 * 3600 * 1000;

    public static readonly TimeSpan DefaultBaseDelay = TimeSpan.FromSeconds(2);
}
