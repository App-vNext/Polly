// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.CircuitBreaker;

namespace Polly;

public static class CircuitBreakerCompositeStrategyBuilderExtensions
{
    public static CompositeStrategyBuilder AddCircuitBreaker(this CompositeStrategyBuilder builder, CircuitBreakerStrategyOptions options);
    public static CompositeStrategyBuilder<TResult> AddCircuitBreaker<TResult>(this CompositeStrategyBuilder<TResult> builder, CircuitBreakerStrategyOptions<TResult> options);
}
