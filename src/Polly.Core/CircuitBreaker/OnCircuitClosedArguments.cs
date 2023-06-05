namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnClosed"/> event.
/// </summary>
/// <param name="IsManual">Indicates whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.</param>
public readonly record struct OnCircuitClosedArguments(bool IsManual);
