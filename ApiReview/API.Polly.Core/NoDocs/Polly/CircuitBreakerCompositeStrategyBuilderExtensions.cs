// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.CircuitBreaker;

namespace Polly;

public static class CircuitBreakerCompositeStrategyBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CircuitBreakerStrategyOptions))]
    public static CompositeStrategyBuilder AddCircuitBreaker(this CompositeStrategyBuilder builder, CircuitBreakerStrategyOptions options);
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    public static CompositeStrategyBuilder<TResult> AddCircuitBreaker<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this CompositeStrategyBuilder<TResult> builder, CircuitBreakerStrategyOptions<TResult> options);
}
