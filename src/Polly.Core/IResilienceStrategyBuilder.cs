namespace Polly;

public interface IResilienceStrategyBuilder
{
    ResilienceStrategyBuilderOptions Options { get; set; }

    IResilienceStrategyBuilder AddStrategy(IResilienceStrategy strategy, ResilienceStrategyOptions? properties = null);

    IResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions? properties = null);

    IResilienceStrategy Create();
}
