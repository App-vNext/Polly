namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.OnRetry"/> for handling the retry event.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnRetryArguments<TResult> : IOutcomeArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnRetryArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="context">The context in which the resilience operation or event occurred.</param>
    /// <param name="outcome">The outcome of the resilience operation or event.</param>
    /// <param name="attemptNumber">The zero-based attempt number.</param>
    /// <param name="retryDelay">The delay before the next retry.</param>
    /// <param name="duration">The duration of this attempt.</param>
    public OnRetryArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber, TimeSpan retryDelay, TimeSpan duration)
    {
        Context = context;
        Outcome = outcome;
        AttemptNumber = attemptNumber;
        RetryDelay = retryDelay;
        Duration = duration;
    }

    /// <summary>
    /// Gets the outcome that will be retried.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context of this event.
    /// </summary>
    public ResilienceContext Context { get; }

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
