// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;

namespace Polly;

public static class ResiliencePipelineBuilderExtensions
{
    public static TBuilder AddPipeline<TBuilder>(this TBuilder builder, ResiliencePipeline pipeline) where TBuilder : ResiliencePipelineBuilderBase;
    public static ResiliencePipelineBuilder<TResult> AddPipeline<TResult>(this ResiliencePipelineBuilder<TResult> builder, ResiliencePipeline<TResult> pipeline);
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, Func<StrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options) where TBuilder : ResiliencePipelineBuilderBase;
    public static ResiliencePipelineBuilder AddStrategy(this ResiliencePipelineBuilder builder, Func<StrategyBuilderContext, ResilienceStrategy<object>> factory, ResilienceStrategyOptions options);
    public static ResiliencePipelineBuilder<TResult> AddStrategy<TResult>(this ResiliencePipelineBuilder<TResult> builder, Func<StrategyBuilderContext, ResilienceStrategy<TResult>> factory, ResilienceStrategyOptions options);
}
