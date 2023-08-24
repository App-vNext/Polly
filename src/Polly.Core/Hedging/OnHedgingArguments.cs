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
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    /// <param name="duration">The execution duration of hedging attempt or the hedging delay in case the attempt was not finished in time.</param>
    public OnHedgingArguments(ResilienceContext context, Outcome<TResult>? outcome, int attemptNumber, TimeSpan duration)
    {
        Context = context;
        Outcome = outcome;
        AttemptNumber = attemptNumber;
        Duration = duration;
    }

    /// <summary>
    /// Gets the outcome that needs to be hedged, if any.
    /// </summary>
    /// <remarks>If this property is <see langword="null"/>, it's an indication that user-callback or hedged operation did not complete within the hedging delay.</remarks>
    public Outcome<TResult>? Outcome { get; }

    /// <summary>
    /// Gets the context of this event.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the zero-based hedging attempt number.
    /// </summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets the execution duration of hedging attempt or the hedging delay in case the attempt was not finished in time.
    /// </summary>
    public TimeSpan Duration { get; }
}
