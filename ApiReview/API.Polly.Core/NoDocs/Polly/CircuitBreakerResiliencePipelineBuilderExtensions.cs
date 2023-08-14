// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.CircuitBreaker;

namespace Polly;

public static class CircuitBreakerResiliencePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder AddCircuitBreaker(this ResiliencePipelineBuilder builder, CircuitBreakerStrategyOptions options);
    public static ResiliencePipelineBuilder<TResult> AddCircuitBreaker<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, CircuitBreakerStrategyOptions<TResult> options);
}
