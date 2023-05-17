using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions.ShouldRetry"/> for determining whether a retry should be performed.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Attempt">The zero-based attempt number. The first attempt is 0, the second attempt is 1, and so on.</param>
public readonly record struct ShouldRetryArguments(ResilienceContext Context, int Attempt) : IResilienceArguments;
