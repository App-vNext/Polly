namespace Polly;

public class ResilienceStrategyBuilderContext
{
    public ResilienceStrategyBuilderContext(
        ResilienceStrategyBuilderOptions builderOptions,
        ResilienceStrategyOptions strategyOptions)
    {
        BuilderOptions = builderOptions;
        StrategyOptions = strategyOptions;
    }

    public ResilienceStrategyBuilderOptions BuilderOptions { get; }

    public ResilienceStrategyOptions StrategyOptions { get; }
}
