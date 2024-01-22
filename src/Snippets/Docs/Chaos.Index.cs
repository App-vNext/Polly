using System.Net.Http;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Simmy;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void Usage()
    {
        #region chaos-usage

        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

        // First, configure regular resilience strategies
        builder
            .AddConcurrencyLimiter(10, 100)
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage> { /* configure options */ })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage> { /* configure options */ })
            .AddTimeout(TimeSpan.FromSeconds(5));

        // Finally, configure chaos strategies if you want to inject chaos.
        // These should come after the regular resilience strategies.

        // 2% of invocations will be injected with chaos
        const double InjectionRate = 0.02;

        builder
            .AddChaosLatency(InjectionRate, TimeSpan.FromMinutes(1)) // Inject a chaos latency to executions
            .AddChaosFault(InjectionRate, () => new InvalidOperationException("Injected by chaos strategy!")) // Inject a chaos fault to executions
            .AddChaosResult(InjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)) // Inject a chaos outcome to executions
            .AddChaosBehavior(0.001, cancellationToken => RestartRedisAsync(cancellationToken)); // Inject a chaos behavior to executions

        #endregion
    }

    private static ValueTask RestartRedisAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
