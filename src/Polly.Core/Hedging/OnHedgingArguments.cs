namespace Polly.Hedging;

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
/// <param name="HasOutcome">
/// Determines whether the outcome is available before loading the next hedged task.
/// No outcome indicates that the previous action did not finish within the hedging delay.
/// </param>
public record OnHedgingArguments(int Attempt, bool HasOutcome);
