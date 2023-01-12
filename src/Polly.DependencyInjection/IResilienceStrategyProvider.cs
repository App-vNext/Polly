namespace Polly;

public interface IResilienceStrategyProvider
{
    IResilienceStrategy GetResilienceStrategy(string strategyName);
}
