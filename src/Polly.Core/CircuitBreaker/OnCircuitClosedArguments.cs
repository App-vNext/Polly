using Polly.Strategy;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnClosed"/> event.
/// </summary>
public readonly struct OnCircuitClosedArguments : IResilienceArguments
{
    internal OnCircuitClosedArguments(ResilienceContext context, bool isManual)
    {
        Context = context;
        IsManual = isManual;
    }

    /// <summary>
    /// Gets a value indicating whether the circuit was closed manually by using <see cref="CircuitBreakerManualControl"/>.
    /// </summary>
    public bool IsManual { get; }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
