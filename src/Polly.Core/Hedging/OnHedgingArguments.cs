using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Represents arguments used by the on-hedging event.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
public readonly record struct OnHedgingArguments(ResilienceContext Context, int Attempt) : IResilienceArguments;
