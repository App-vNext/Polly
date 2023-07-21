namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnClosed"/> event.
/// </summary>
public sealed class OnCircuitClosedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitClosedArguments"/> class.
    /// </summary>
    /// <param name="isManual">Indicates whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.</param>
    public OnCircuitClosedArguments(bool isManual) => IsManual = isManual;

    /// <summary>
    /// Gets a value indicating whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.
    /// </summary>
    public bool IsManual { get; }
}
