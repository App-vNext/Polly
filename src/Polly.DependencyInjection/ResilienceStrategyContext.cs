namespace Polly;

public class ResilienceStrategyContext
{
    public ResilienceStrategyContext(IServiceProvider serviceProvider, IResilienceStrategyBuilder builder)
    {
        ServiceProvider = serviceProvider;
        Builder = builder;
    }

    public IServiceProvider ServiceProvider { get; }

    public IResilienceStrategyBuilder Builder { get; }

}
