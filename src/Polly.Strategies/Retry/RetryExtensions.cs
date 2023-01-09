namespace Polly.Retry;

public static class RetryExtensions
{
    public static IResilienceStrategyBuilder AddRetry(this IResilienceStrategyBuilder builder, RetryStrategyOptions options) 
        => builder.AddStrategy(context => new RetryStrategy(options), options);
}
