using Polly.Utils;

namespace Polly.TestUtils;

public static class ResilienceStrategyExtensions
{
    public static ResiliencePipeline AsPipeline(this ResilienceStrategy strategy) => new ResiliencePipelineBridge(strategy);

    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        return builder.AddPipeline(strategy.AsPipeline());
    }
}
