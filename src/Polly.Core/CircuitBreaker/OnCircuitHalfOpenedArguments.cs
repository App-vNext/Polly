using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnHalfOpened"/> event.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct OnCircuitHalfOpenedArguments(ResilienceContext Context) : IResilienceArguments;
