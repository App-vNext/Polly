namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnOpened"/> event.
/// </summary>
/// <param name="BreakDuration">The duration of break.</param>
/// <param name="IsManual">Indicates whether the circuit was opened manually by using <see cref="CircuitBreakerManualControl"/>.</param>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly record struct OnCircuitOpenedArguments(TimeSpan BreakDuration, bool IsManual);
