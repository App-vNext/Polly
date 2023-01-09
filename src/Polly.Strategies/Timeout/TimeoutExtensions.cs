namespace Polly.Timeout;

public static class TimeoutExtensions
{
    public static IResilienceStrategyBuilder AddTimeout(this IResilienceStrategyBuilder builder, TimeoutStrategyOptions options, ResilienceStrategyOptions? properties = null)
    {
        return builder.AddStrategy(context => new TimeoutStrategy(options), properties);
    }
}
