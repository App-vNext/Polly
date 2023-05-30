using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.ShouldHandle"/> predicate.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct CircuitBreakerPredicateArguments(ResilienceContext Context) : IResilienceArguments;
