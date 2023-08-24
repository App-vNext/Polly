namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.ShouldHandle"/> predicate.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct CircuitBreakerPredicateArguments<TResult> : IOutcomeArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerPredicateArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    public CircuitBreakerPredicateArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    /// <summary>
    /// Gets the outcome of user-callback.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context of this event.
    /// </summary>
    public ResilienceContext Context { get; }
}
