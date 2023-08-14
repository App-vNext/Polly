// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Fallback;

namespace Polly;

public static class FallbackResiliencePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder<TResult> AddFallback<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, FallbackStrategyOptions<TResult> options);
}
