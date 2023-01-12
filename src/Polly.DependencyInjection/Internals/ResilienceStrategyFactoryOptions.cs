namespace Polly.Internals;

internal class ResilienceStrategyFactoryOptions
{
    public List<Action<ResilienceStrategyContext>> ConfigureActions { get; } = new();
}
