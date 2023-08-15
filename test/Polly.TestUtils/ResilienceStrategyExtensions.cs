using Polly.Utils;

namespace Polly.TestUtils;

public static class ResilienceStrategyExtensions
{
    public static ResiliencePipeline AsPipeline(this ResilienceStrategy strategy)
        => new ResiliencePipelineBuilder().AddStrategy(strategy).Build();

    public static ResiliencePipeline<T> AsPipeline<T>(this ResilienceStrategy<T> strategy)
        => new ResiliencePipelineBuilder<T>().AddStrategy(strategy).Build();

    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        return builder.AddStrategy(_ => strategy, new TestResilienceStrategyOptions());
    }

    public static ResiliencePipelineBuilder<T> AddStrategy<T>(this ResiliencePipelineBuilder<T> builder, ResilienceStrategy<T> strategy)
    {
        return builder.AddStrategy(_ => strategy, new TestResilienceStrategyOptions());
    }
}
