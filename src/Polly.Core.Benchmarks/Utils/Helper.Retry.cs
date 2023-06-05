using System;
using Polly.Strategy;

namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreateRetry(PollyVersion technology)
    {
        var delay = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 =>
                Policy
                    .HandleResult(Failure)
                    .Or<InvalidOperationException>()
                    .WaitAndRetryAsync(3, attempt => delay, (_, _) => Task.CompletedTask),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                builder.AddRetry(new RetryStrategyOptions<string>
                {
                    RetryCount = 3,
                    BackoffType = RetryBackoffType.Constant,
                    BaseDelay = delay,
                    ShouldRetry = args => args switch
                    {
                        { Exception: InvalidOperationException } => PredicateResult.True,
                        { Result: string result } when result == Failure => PredicateResult.True,
                        _ => PredicateResult.False
                    },
                    OnRetry = _ => default
                });
            }),
            _ => throw new NotSupportedException()
        };
    }
}
