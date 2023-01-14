using Polly.Timeout;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Options;

internal static partial class Helper
{
    public static object CraeteSimplePipeline(PollyVersion technology)
    {
        var delay = TimeSpan.FromSeconds(10);
        var innerTimeout = TimeSpan.FromSeconds(10);
        var outerTimeout = TimeSpan.FromSeconds(30);

        return technology switch
        {
            PollyVersion.V7 =>
                Policy.WrapAsync(
                    Policy.TimeoutAsync(innerTimeout, TimeoutStrategy.Optimistic, (_, _, _) => Task.CompletedTask).AsAsyncPolicy<int>(),
                    Policy
                        .HandleResult(10)
                        .Or<InvalidOperationException>()
                        .WaitAndRetryAsync(3, attempt => delay, (result, time) => Task.CompletedTask),
                    Policy.TimeoutAsync(outerTimeout, TimeoutStrategy.Optimistic, (_, _, _) => Task.CompletedTask).AsAsyncPolicy<int>()),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                builder.AddTimeout(new TimeoutStrategyOptions
                {
                   TimeoutInterval = outerTimeout,
                   OnTimeout = new Events<OnTimeoutArguments>().Add(args => new ValueTask()),
                   StrategyName = "outer-timeout"
                });

                builder.AddRetry(new RetryStrategyOptions
                {
                    ShouldRetry = new Predicates<ShouldRetryArguments>()
                        .Add(10)
                        .AddException<InvalidOperationException>(),
                    RetryCount = 3,
                    RetryDelayGenerator = attempt => delay,
                    OnRetry = new Events<RetryActionArguments>().Add((args) => default(ValueTask)),
                    StrategyName = "retries"
                });

                builder.AddTimeout(new TimeoutStrategyOptions
                {
                    TimeoutInterval = innerTimeout,
                    OnTimeout = new Events<OnTimeoutArguments>().Add(args => new ValueTask()),
                    StrategyName = "inner-timeout"
                });
            }),
            _ => throw new NotImplementedException()
        };
    }
}
