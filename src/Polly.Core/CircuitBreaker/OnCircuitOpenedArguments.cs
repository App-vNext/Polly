using Polly.Strategy;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnOpened"/> event.
/// </summary>
public readonly struct OnCircuitOpenedArguments : IResilienceArguments
{
    internal OnCircuitOpenedArguments(ResilienceContext context, TimeSpan breakDuration, bool isManual)
    {
        BreakDuration = breakDuration;
        IsManual = isManual;
        Context = context;
    }

    /// <summary>
    /// Gets the duration of break.
    /// </summary>
    public TimeSpan BreakDuration { get; }

    /// <summary>
    /// Gets a value indicating whether the circuit was opened manually by using <see cref="CircuitBreakerManualControl"/>.
    /// </summary>
    public bool IsManual { get; }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}

