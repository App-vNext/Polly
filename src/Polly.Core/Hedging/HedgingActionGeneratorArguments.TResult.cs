namespace Polly.Hedging;

/// <summary>
/// Represents arguments used in the hedging resilience strategy.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
/// <param name="Callback">The callback passed to hedging strategy.</param>
///
public readonly record struct HedgingActionGeneratorArguments<TResult>(ResilienceContext Context, int Attempt, Func<ResilienceContext, ValueTask<Outcome<TResult>>> Callback);
