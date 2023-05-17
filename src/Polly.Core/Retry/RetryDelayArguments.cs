using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions.RetryDelayGenerator"/> for generating the next retry delay.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Attempt">The zero-based attempt number. The first attempt is 0, the second attempt is 1, and so on.</param>
/// <param name="DelayHint">The delay suggested by the retry strategy.</param>
public readonly record struct RetryDelayArguments(ResilienceContext Context, int Attempt, TimeSpan DelayHint) : IResilienceArguments;
