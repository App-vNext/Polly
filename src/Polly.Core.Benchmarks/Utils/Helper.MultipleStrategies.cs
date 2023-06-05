using System.Threading.RateLimiting;
using Polly.Strategy;

namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreateStrategyPipeline(PollyVersion technology) => technology switch
    {
        PollyVersion.V7 => Policy.WrapAsync(
            Policy.HandleResult(Failure).Or<InvalidOperationException>().AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(30), 10, TimeSpan.FromSeconds(5)),
            Policy.TimeoutAsync<string>(TimeSpan.FromSeconds(1)),
            Policy.Handle<InvalidOperationException>().OrResult(Failure).WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(1)),
            Policy.TimeoutAsync<string>(TimeSpan.FromSeconds(10)),
            Policy.BulkheadAsync<string>(10, 10)),
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
                    predicate => predicate.Handle<InvalidOperationException>().HandleResult(Failure),
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
                    ShouldHandle = args => args switch
                    {
                        { Exception: InvalidOperationException } => PredicateResult.True,
                        { Result: string result } when result == Failure => PredicateResult.True,
                        _ => PredicateResult.False
                    }
                });
        }),
        _ => throw new NotSupportedException()
    };
}
