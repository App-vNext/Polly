namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.OnRetry"/> for handling the retry event.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnRetryArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnRetryArguments"/> struct.
    /// </summary>
    /// <param name="attemptNumber">The zero-based attempt number.</param>
    /// <param name="retryDelay">The delay before the next retry.</param>
    /// <param name="duration">The duration of this attempt.</param>
    public OnRetryArguments(int attemptNumber, TimeSpan retryDelay, TimeSpan duration)
    {
        AttemptNumber = attemptNumber;
        RetryDelay = retryDelay;
        Duration = duration;
    }

    /// <summary>
    /// Gets the zero-based attempt number.
    /// </summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets the delay before the next retry.
    /// </summary>
    public TimeSpan RetryDelay { get; }

    /// <summary>
    /// Gets the duration of this attempt.
    /// </summary>
    public TimeSpan Duration { get; }
}
