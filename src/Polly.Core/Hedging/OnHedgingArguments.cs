namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnHedgingArguments<TResult> : IOutcomeArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnHedgingArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    /// <param name="hasOutcome">Indicates whether outcome is available.</param>
    /// <param name="duration">The execution duration of hedging attempt or the hedging delay in case the attempt was not finished in time.</param>
    public OnHedgingArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber, bool hasOutcome, TimeSpan duration)
    {
        Context = context;
        Outcome = outcome;
        AttemptNumber = attemptNumber;
        HasOutcome = hasOutcome;
        Duration = duration;
    }

    /// <summary>
    /// Gets the outcome of the resilience operation or event.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context in which the resilience operation or event occurred.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the zero-based hedging attempt number.
    /// </summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets a value indicating whether the outcome is available before loading the next hedged task.
    /// </summary>
    /// <remarks>
    /// No outcome indicates that the previous action did not finish within the hedging delay.
    /// </remarks>
    public bool HasOutcome { get; }

    /// <summary>
    /// Gets the execution duration of hedging attempt or the hedging delay in case the attempt was not finished in time.
    /// </summary>
    public TimeSpan Duration { get; }
}
