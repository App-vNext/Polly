namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.OnRetry"/> for handling the retry event.
/// </summary>
public sealed class OnRetryArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnRetryArguments"/> class.
    /// </summary>
    /// <param name="attempt">The zero-based attempt number.</param>
    /// <param name="retryDelay">The delay before the next retry.</param>
    /// <param name="executionTime">The execution time of this attempt.</param>
    public OnRetryArguments(int attempt, TimeSpan retryDelay, TimeSpan executionTime)
    {
        Attempt = attempt;
        RetryDelay = retryDelay;
        ExecutionTime = executionTime;
    }

    /// <summary>
    /// Gets the zero-based attempt number.
    /// </summary>
    public int Attempt { get; }

    /// <summary>
    /// Gets the delay before the next retry.
    /// </summary>
    public TimeSpan RetryDelay { get; }

    /// <summary>
    /// Gets the execution time of this attempt.
    /// </summary>
    public TimeSpan ExecutionTime { get; }
}
