using Polly.Strategy;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.OnHalfOpen"/> event.
/// </summary>
public readonly struct OnCircuitHalfOpenArguments : IResilienceArguments
{
    internal OnCircuitHalfOpenArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
