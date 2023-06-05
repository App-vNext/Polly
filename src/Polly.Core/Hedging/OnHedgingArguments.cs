namespace Polly.Hedging;

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
public readonly record struct OnHedgingArguments(int Attempt);
