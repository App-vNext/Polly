namespace Polly.Hedging;

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly record struct OnHedgingArguments(int Attempt);
