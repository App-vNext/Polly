using System;
using Polly.Strategy;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public static object CreateRetry(PollyVersion technology)
    {
        var delay = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 =>
                Policy
                    .HandleResult(10)
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(3, attempt => delay, (_, _) => Task.CompletedTask),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    RetryCount = 3,
                    BackoffType = RetryBackoffType.Constant,
                    BaseDelay = delay,
                    ShouldRetry = (outcome, _) => outcome switch
                    {
                        { Exception: HttpRequestException error } => PredicateResult.True,
                        { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True,
                        _ => PredicateResult.False
                    },
                    OnRetry = (_, _) => default
                });
            }),
            _ => throw new NotSupportedException()
        };
    }
}
