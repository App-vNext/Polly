// Assembly 'Polly.Core'

namespace Polly;

public static class ResilienceStrategyBuilderExtensions
{
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy) where TBuilder : ResilienceStrategyBuilderBase;
}
