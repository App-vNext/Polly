using Polly.Strategy;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnBreak"/> event.
/// </summary>
public readonly struct OnCircuitBreakArguments : IResilienceArguments
{
    internal OnCircuitBreakArguments(ResilienceContext context, TimeSpan breakDuration)
    {
        BreakDuration = breakDuration;
        Context = context;
    }

    /// <summary>
    /// Gets the duration of break.
    /// </summary>
    public TimeSpan BreakDuration { get; }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
