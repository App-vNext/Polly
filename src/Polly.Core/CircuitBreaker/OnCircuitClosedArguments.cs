using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions.OnClosed"/> event.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="IsManual">Indicates whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.</param>
public readonly record struct OnCircuitClosedArguments(ResilienceContext Context, bool IsManual) : IResilienceArguments;
