using Polly;
using Polly.Strategies;

namespace Resilience.Polly;

public static class ResilienceStrategyExtensions
{
    // IResilienceStrategy -> Polly extensions

    public static IAsyncPolicy AsAsyncPolicy(this IResilienceStrategy strategy) => new StrategyAsyncPolicy(strategy);

    public static IAsyncPolicy<T> AsAsyncPolicy<T>(this IResilienceStrategy strategy) => new StrategyAsyncPolicy<T>(strategy);

    public static ISyncPolicy AsSyncPolicy(this IResilienceStrategy strategy) => new StrategySyncPolicy(strategy);

    public static ISyncPolicy<T> AsSyncPolicy<T>(this IResilienceStrategy strategy) => new StrategySyncPolicy<T>(strategy);

    public static ResilienceContext Update(this ResilienceContext context, Context pollyContext) =>
        // TODO: add the conversion logic
        context;
}
