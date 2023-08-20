using Polly.Utils;

namespace Polly.TestUtils;

public static class NonReactiveStrategyExtensions
{
    public static ResilienceStrategy AsStrategy(this NonReactiveResilienceStrategy strategy) => new NonReactiveResilienceStrategyBridge(strategy);

    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, NonReactiveResilienceStrategy strategy)
        where TBuilder : CompositeStrategyBuilderBase
    {
        return builder.AddStrategy(strategy.AsStrategy());
    }
}
