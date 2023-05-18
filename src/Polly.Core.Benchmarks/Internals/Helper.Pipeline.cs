using System.Threading.RateLimiting;
using Polly;
using Polly.Strategy;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public static object CreateStrategyPipeline(PollyVersion technology) => technology switch
    {
        PollyVersion.V7 => Policy.WrapAsync(
            Policy.HandleResult(10).Or<InvalidOperationException>().AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(30), 10, TimeSpan.FromSeconds(5)),
            Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(1)),
            Policy.Handle<InvalidOperationException>().OrResult(10).WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(1)),
            Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(10)),
            Policy.BulkheadAsync<int>(10, 10)),
        PollyVersion.V8 => CreateStrategy(builder =>
        {
            builder
                .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
                {
                    QueueLimit = 10,
                    PermitLimit = 10
                })
                .AddTimeout(TimeSpan.FromSeconds(10))
                .AddRetry(
                    (outcome, _) => outcome switch
                    {
                        (int result, _) when result == 10 => true,
                        (_, InvalidOperationException) => true,
                        _ => false
                    },
                    RetryBackoffType.Constant,
                    3,
                    TimeSpan.FromSeconds(1))
                .AddTimeout(TimeSpan.FromSeconds(1))
                .AddAdvancedCircuitBreaker(new()
                {
                    FailureThreshold = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(5),
                    ShouldHandle = new OutcomePredicate<CircuitBreakerPredicateArguments>()
                        .HandleOutcome<int>((outcome, _) => outcome.Result == 10 || outcome.Exception is InvalidOperationException)
                });
        }),
        _ => throw new NotSupportedException()
    };
}
