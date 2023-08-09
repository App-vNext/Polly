// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Retry;

namespace Polly;

public static class RetryCompositeStrategyBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RetryStrategyOptions))]
    public static CompositeStrategyBuilder AddRetry(this CompositeStrategyBuilder builder, RetryStrategyOptions options);
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    public static CompositeStrategyBuilder<TResult> AddRetry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this CompositeStrategyBuilder<TResult> builder, RetryStrategyOptions<TResult> options);
}
