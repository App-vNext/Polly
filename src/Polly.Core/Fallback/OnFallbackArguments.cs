namespace Polly.Fallback;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in fallback handling scenarios.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnFallbackArguments<TResult> : IOutcomeArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnFallbackArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    public OnFallbackArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Outcome = outcome;
        Context = context;
    }

    /// <summary>
    /// Gets the outcome that caused the fallback to be executed.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context of this event.
    /// </summary>
    public ResilienceContext Context { get; }
}
