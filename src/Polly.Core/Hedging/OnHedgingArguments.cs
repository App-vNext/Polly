namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
public readonly struct OnHedgingArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnHedgingArguments"/> struct.
    /// </summary>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    /// <param name="hasOutcome">Indicates whether outcome is available.</param>
    /// <param name="duration">The execution duration of hedging attempt or the hedging delay in case the attempt was not finished in time.</param>
    public OnHedgingArguments(int attemptNumber, bool hasOutcome, TimeSpan duration)
    {
        AttemptNumber = attemptNumber;
        HasOutcome = hasOutcome;
        Duration = duration;
    }

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
