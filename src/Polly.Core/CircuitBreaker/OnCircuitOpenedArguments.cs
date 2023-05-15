using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnOpened"/> event.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="BreakDuration">The duration of break.</param>
/// <param name="IsManual">Indicates whether the circuit was opened manually by using <see cref="CircuitBreakerManualControl"/>.</param>
public readonly record struct OnCircuitOpenedArguments(ResilienceContext Context, TimeSpan BreakDuration, bool IsManual) : IResilienceArguments;
