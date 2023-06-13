namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnClosed"/> event.
/// </summary>
/// <param name="IsManual">Indicates whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.</param>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly record struct OnCircuitClosedArguments(bool IsManual);
