using Polly.Timeout.Internals;

namespace Polly.Timeout;

public static class TimeoutExtensions
{
    public static IResilienceStrategyBuilder AddTimeout(this IResilienceStrategyBuilder builder, TimeSpan timeoutInterval) => builder.AddTimeout(new TimeoutStrategyOptions { TimeoutInterval = timeoutInterval });

    public static IResilienceStrategyBuilder AddTimeout(this IResilienceStrategyBuilder builder, TimeoutStrategyOptions options) => builder.AddStrategy(context => new TimeoutStrategy(options), options);
}
