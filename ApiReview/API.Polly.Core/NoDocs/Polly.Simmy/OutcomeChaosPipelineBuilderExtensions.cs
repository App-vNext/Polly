// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Outcomes;

namespace Polly.Simmy;

public static class OutcomeChaosPipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Exception fault);
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Func<Exception?> faultGenerator);
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, OutcomeStrategyOptions<Exception> options);
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Exception fault);
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Func<Exception?> faultGenerator);
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, OutcomeStrategyOptions<Exception> options);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, TResult result);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Func<TResult?> outcomeGenerator);
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, OutcomeStrategyOptions<TResult> options);
}
