namespace Polly;

public interface IResilienceStrategyBuilder
{
    ResilienceStrategyBuilderOptions Options { get; set; }

    IResilienceStrategyBuilder AddStrategy(IResilienceStrategy strategy, ResilienceStrategyOptions? options = null);

    IResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions? options = null);

    IResilienceStrategy Build();
}
