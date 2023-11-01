// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Outcomes;

namespace Polly.Simmy;

public static class OutcomePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, double injectionRate, TResult result);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, double injectionRate, Func<TResult?> resultGenerator);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(this ResiliencePipelineBuilder<TResult> builder, OutcomeStrategyOptions<TResult> options);
}
