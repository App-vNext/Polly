using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Represents arguments used in hedging handling scenarios.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct HandleHedgingArguments(ResilienceContext Context) : IResilienceArguments;
