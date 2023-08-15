// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;

namespace Polly;

public static class HedgingResiliencePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder<TResult> AddHedging<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, HedgingStrategyOptions<TResult> options);
}