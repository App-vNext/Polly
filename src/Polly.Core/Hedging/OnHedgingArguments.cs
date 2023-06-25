namespace Polly.Hedging;

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
/// <param name="HasOutcome">
/// Determines whether the outcome is available before loading the next hedged task.
/// No outcome indicates that the previous action did not finish within the hedging delay.
/// </param>
/// <param name="ExecutionTime">
/// The execution time of hedging attempt or the hedging delay
/// in case the attempt was not finished in time.
/// </param>
public record OnHedgingArguments(int Attempt, bool HasOutcome, TimeSpan ExecutionTime);
