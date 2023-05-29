namespace Polly.Retry;

/// <inheritdoc/>
public class RetryStrategyOptions : RetryStrategyOptions<object>
{
    /// <summary>
    /// Value that represents infinite retries.
    /// </summary>
    public const int InfiniteRetryCount = RetryConstants.InfiniteRetryCount;
}
