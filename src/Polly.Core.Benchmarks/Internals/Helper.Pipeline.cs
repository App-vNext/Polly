using System.Threading.RateLimiting;
using Polly;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public static object CreateStrategyPipeline(PollyVersion technology) => technology switch
    {
        PollyVersion.V7 => Policy.WrapAsync(
            Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(1)),
            Policy.Handle<InvalidOperationException>().OrResult<int>(10).WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(1)),
            Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(10)),
            Policy.BulkheadAsync<int>(10, 10)),
        PollyVersion.V8 => CreateStrategy(builder =>
        {
            builder
                .AddTimeout(TimeSpan.FromSeconds(1))
                .AddRetry(
                    predicate => predicate.HandleException<InvalidOperationException>().HandleResult(10),
                    RetryBackoffType.Constant,
                    3,
                    TimeSpan.FromSeconds(1))
                .AddTimeout(TimeSpan.FromSeconds(10))
                .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
                {
                    QueueLimit = 10,
                    PermitLimit = 10
                });
        }),
        _ => throw new NotSupportedException()
    };
}
