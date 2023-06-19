namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.OnRetry"/> for handling the retry event.
/// </summary>
/// <param name="Attempt">The zero-based attempt number. The first attempt is 0, the second attempt is 1, and so on.</param>
/// <param name="RetryDelay">The delay before the next retry.</param>
public record OnRetryArguments(int Attempt, TimeSpan RetryDelay);
