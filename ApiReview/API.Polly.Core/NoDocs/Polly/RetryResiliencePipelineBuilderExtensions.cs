// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Retry;

namespace Polly;

public static class RetryResiliencePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder AddRetry(this ResiliencePipelineBuilder builder, RetryStrategyOptions options);
    public static ResiliencePipelineBuilder<TResult> AddRetry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, RetryStrategyOptions<TResult> options);
}
