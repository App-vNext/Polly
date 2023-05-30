using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.OnRetry"/> for handling the retry event.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Attempt">The zero-based attempt number. The first attempt is 0, the second attempt is 1, and so on.</param>
/// <param name="RetryDelay">The delay before the next retry.</param>
public readonly record struct OnRetryArguments(ResilienceContext Context, int Attempt, TimeSpan RetryDelay) : IResilienceArguments;
