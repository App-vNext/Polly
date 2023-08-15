namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnOpened"/> event.
/// </summary>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly struct OnCircuitOpenedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitOpenedArguments"/> struct.
    /// </summary>
    /// <param name="breakDuration">The duration of break.</param>
    /// <param name="isManual">Indicates whether the circuit was opened manually by using <see cref="CircuitBreakerManualControl"/>.</param>
    public OnCircuitOpenedArguments(TimeSpan breakDuration, bool isManual)
    {
        BreakDuration = breakDuration;
        IsManual = isManual;
    }

    /// <summary>
    /// Gets the duration of break.
    /// </summary>
    public TimeSpan BreakDuration { get; }

    /// <summary>
    /// Gets a value indicating whether the circuit was opened manually by using <see cref="CircuitBreakerManualControl"/>.
    /// </summary>
    public bool IsManual { get; }
}
