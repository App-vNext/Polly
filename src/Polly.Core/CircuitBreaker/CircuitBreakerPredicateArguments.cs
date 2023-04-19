using Polly.Strategy;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="BaseCircuitBreakerStrategyOptions.ShouldHandle"/> predicate.
/// </summary>
public readonly struct CircuitBreakerPredicateArguments : IResilienceArguments
{
    internal CircuitBreakerPredicateArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
