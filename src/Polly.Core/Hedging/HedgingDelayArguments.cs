using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Arguments used by hedging delay generator.
/// </summary>
/// <param name="Context">The context associated with the execution of user-provided callback.</param>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
public readonly record struct HedgingDelayArguments(ResilienceContext Context, int Attempt) : IResilienceArguments;
