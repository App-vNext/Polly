// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Retry;

namespace Polly;

public static class RetryResiliencePipelineBuilderExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RetryStrategyOptions))]
    public static ResiliencePipelineBuilder AddRetry(this ResiliencePipelineBuilder builder, RetryStrategyOptions options);
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members preserved.")]
    public static ResiliencePipelineBuilder<TResult> AddRetry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, RetryStrategyOptions<TResult> options);
}
