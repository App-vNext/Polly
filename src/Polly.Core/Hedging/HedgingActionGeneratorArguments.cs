namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in the hedging resilience strategy.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>
/// The <see cref="PrimaryContext"/> represents the context that was received by the hedging strategy and used to execute the primary action.
/// To prevent race conditions, the hedging strategy then clones the primary context into <see cref="ActionContext"/> and uses it to execute the hedged action.
/// Every hedged action gets its own context that is cloned from the primary.
/// <para>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </para>
/// </remarks>
public readonly struct HedgingActionGeneratorArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HedgingActionGeneratorArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="primaryContext">The primary context received by hedging strategy.</param>
    /// <param name="actionContext">The action context. cloned from the primary context.</param>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    /// <param name="callback">The callback passed to the hedging strategy.</param>
    public HedgingActionGeneratorArguments(
        ResilienceContext primaryContext,
        ResilienceContext actionContext,
        int attemptNumber,
        Func<ResilienceContext, ValueTask<Outcome<TResult>>> callback)
    {
        PrimaryContext = primaryContext;
        ActionContext = actionContext;
        AttemptNumber = attemptNumber;
        Callback = callback;
    }

    /// <summary>
    /// Gets the primary resilience context as received by the hedging strategy.
    /// </summary>
    public ResilienceContext PrimaryContext { get; }

    /// <summary>
    /// Gets the action context that will be used for the hedged action.
    /// </summary>
    /// <remarks>
    /// This context is cloned from <see cref="PrimaryContext"/>.
    /// </remarks>
    public ResilienceContext ActionContext { get; }

    /// <summary>
    /// Gets the zero-based hedging attempt number.
    /// </summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets the callback passed to the hedging strategy.
    /// </summary>
    public Func<ResilienceContext, ValueTask<Outcome<TResult>>> Callback { get; }
}
