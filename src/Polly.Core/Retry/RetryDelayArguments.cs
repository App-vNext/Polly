namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.RetryDelayGenerator"/> for generating the next retry delay.
/// </summary>
/// <param name="Attempt">The zero-based attempt number. The first attempt is 0, the second attempt is 1, and so on.</param>
/// <param name="DelayHint">The delay suggested by the retry strategy.</param>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly record struct RetryDelayArguments(int Attempt, TimeSpan DelayHint);
