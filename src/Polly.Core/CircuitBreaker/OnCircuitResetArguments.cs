using Polly.Strategy;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnReset"/> event.
/// </summary>
public readonly struct OnCircuitResetArguments : IResilienceArguments
{
    internal OnCircuitResetArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
