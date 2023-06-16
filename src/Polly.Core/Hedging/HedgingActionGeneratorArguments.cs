namespace Polly.Hedging;

/// <summary>
/// Represents arguments used in the hedging resilience strategy.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="PrimaryContext">The primary resilience context.</param>
/// <param name="Context">The context associated with the execution of a user-provided callback that is cloned from the primary context.</param>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
/// <param name="Callback">The callback passed to hedging strategy.</param>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly record struct HedgingActionGeneratorArguments<TResult>(
    ResilienceContext PrimaryContext,
    ResilienceContext Context,
    int Attempt,
    Func<ResilienceContext, ValueTask<Outcome<TResult>>> Callback);
