namespace Polly.Fallback;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in fallback handling scenarios.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct FallbackPredicateArguments<TResult> : IOutcomeArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FallbackPredicateArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    public FallbackPredicateArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    /// <summary>
    /// Gets the outcome of the resilience operation or event.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context in which the resilience operation or event occurred.
    /// </summary>
    public ResilienceContext Context { get; }
}
