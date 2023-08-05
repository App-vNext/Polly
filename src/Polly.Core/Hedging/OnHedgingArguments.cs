namespace Polly.Hedging;

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
public sealed class OnHedgingArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnHedgingArguments"/> class.
    /// </summary>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    /// <param name="hasOutcome">Indicates whether outcome is available.</param>
    /// <param name="executionTime">The execution time of hedging attempt or the hedging delay in case the attempt was not finished in time.</param>
    public OnHedgingArguments(int attemptNumber, bool hasOutcome, TimeSpan executionTime)
    {
        AttemptNumber = attemptNumber;
        HasOutcome = hasOutcome;
        ExecutionTime = executionTime;
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
    /// Gets the execution time of hedging attempt or the hedging delay in case the attempt was not finished in time.
    /// </summary>
    public TimeSpan ExecutionTime { get; }
}
