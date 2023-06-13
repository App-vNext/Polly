namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.ShouldRetry"/> for determining whether a retry should be performed.
/// </summary>
/// <param name="Attempt">The zero-based attempt number. The first attempt is 0, the second attempt is 1, and so on.</param>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly record struct ShouldRetryArguments(int Attempt);
