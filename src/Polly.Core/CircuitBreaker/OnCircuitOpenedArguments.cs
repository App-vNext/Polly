namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnOpened"/> event.
/// </summary>
public sealed class OnCircuitOpenedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitOpenedArguments"/> class.
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
