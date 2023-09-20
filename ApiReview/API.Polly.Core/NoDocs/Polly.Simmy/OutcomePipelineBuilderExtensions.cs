// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Outcomes;

namespace Polly.Simmy;

public static class OutcomePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Exception fault);
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Func<Exception?> faultGenerator);
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, FaultStrategyOptions options);
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Exception fault);
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Func<Exception?> faultGenerator);
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, FaultStrategyOptions options);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, TResult result);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Func<TResult?> outcomeGenerator);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, OutcomeStrategyOptions<TResult> options);
}
