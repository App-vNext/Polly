// Assembly 'Polly.Core'

using System;

namespace Polly;

public static class ResilienceStrategyBuilderExtensions
{
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy) where TBuilder : ResilienceStrategyBuilderBase;
    public static ResilienceStrategyBuilder<TResult> AddStrategy<TResult>(this ResilienceStrategyBuilder<TResult> builder, ResilienceStrategy<TResult> strategy);
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, Func<ResilienceStrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
