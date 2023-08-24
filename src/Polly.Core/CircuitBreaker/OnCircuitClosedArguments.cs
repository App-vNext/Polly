namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnClosed"/> event.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnCircuitClosedArguments<TResult> : IOutcomeArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitClosedArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    /// <param name="isManual">Indicates whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.</param>
    public OnCircuitClosedArguments(ResilienceContext context, Outcome<TResult> outcome, bool isManual)
    {
        Context = context;
        Outcome = outcome;
        IsManual = isManual;
    }

    /// <summary>
    /// Gets the outcome of the resilience operation or event.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context in which the resilience operation or event occurred.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets a value indicating whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.
    /// </summary>
    public bool IsManual { get; }
}
