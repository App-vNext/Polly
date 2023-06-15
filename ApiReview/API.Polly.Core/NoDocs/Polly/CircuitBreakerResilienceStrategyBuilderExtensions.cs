// Assembly 'Polly.Core'

using Polly.CircuitBreaker;

namespace Polly;

public static class CircuitBreakerResilienceStrategyBuilderExtensions
{
    public static ResilienceStrategyBuilder AddAdvancedCircuitBreaker(this ResilienceStrategyBuilder builder, AdvancedCircuitBreakerStrategyOptions options);
    public static ResilienceStrategyBuilder<TResult> AddAdvancedCircuitBreaker<TResult>(this ResilienceStrategyBuilder<TResult> builder, AdvancedCircuitBreakerStrategyOptions<TResult> options);
    public static ResilienceStrategyBuilder<TResult> AddSimpleCircuitBreaker<TResult>(this ResilienceStrategyBuilder<TResult> builder, SimpleCircuitBreakerStrategyOptions<TResult> options);
    public static ResilienceStrategyBuilder AddSimpleCircuitBreaker(this ResilienceStrategyBuilder builder, SimpleCircuitBreakerStrategyOptions options);
}
