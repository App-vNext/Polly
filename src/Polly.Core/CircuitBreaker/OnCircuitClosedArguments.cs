namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnClosed"/> event.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnCircuitClosedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitClosedArguments"/> struct.
    /// </summary>
    /// <param name="isManual">Indicates whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.</param>
    public OnCircuitClosedArguments(bool isManual) => IsManual = isManual;

    /// <summary>
    /// Gets a value indicating whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.
    /// </summary>
    public bool IsManual { get; }
}
