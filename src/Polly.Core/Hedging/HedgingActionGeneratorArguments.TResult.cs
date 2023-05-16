using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Represents arguments used in <see cref="HedgingHandler{TResult}.HedgingActionGenerator"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Attempt">The zero-based hedging attempt number.</param>
public readonly record struct HedgingActionGeneratorArguments<TResult>(ResilienceContext Context, int Attempt) : IResilienceArguments;
