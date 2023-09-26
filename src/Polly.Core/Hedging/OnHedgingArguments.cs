namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnHedgingArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnHedgingArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="primaryContext">The primary context received by hedging strategy.</param>
    /// <param name="actionContext">The action context. cloned from the primary context.</param>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    /// <remarks>
    /// The <paramref name="primaryContext"/> represents the context that was received by the hedging strategy and used to execute the primary action.
    /// To prevent race conditions, hedging strategy then clones the primary context into <paramref name="actionContext"/> and uses it to execute the hedged action.
    /// Every hedged action gets its own context that is cloned from primary.
    /// </remarks>
    public OnHedgingArguments(ResilienceContext primaryContext, ResilienceContext actionContext, int attemptNumber)
    {
        PrimaryContext = primaryContext;
        ActionContext = actionContext;
        AttemptNumber = attemptNumber;
    }

    /// <summary>
    /// Gets the primary resilience context as received by hedging strategy.
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
}
