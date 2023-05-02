using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Represents arguments used in <see cref="HedgingActionGenerator"/>.
/// </summary>
/// <param name="Context">The context associated with the execution of user-provided callback.</param>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
public readonly record struct HedgingActionGeneratorArguments(ResilienceContext Context, int Attempt) : IResilienceArguments;
