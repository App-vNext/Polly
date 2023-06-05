namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnHalfOpened"/> event.
/// </summary>
public readonly record struct OnCircuitHalfOpenedArguments();
