using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Simmy.Latency;

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
            .AddChaosOutcome(InjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)) // Inject a chaos outcome to executions
            .AddChaosBehavior(0.001, cancellationToken => RestartRedisAsync(cancellationToken)); // Inject a chaos behavior to executions

        #endregion
    }

    public static void ApplyChaosSelectively(IServiceCollection services)
    {
        #region chaos-selective

        services.AddResiliencePipeline("chaos-pipeline", (builder, context) =>
        {
            var environment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();

            builder.AddChaosFault(new ChaosFaultStrategyOptions
            {
                EnabledGenerator = args =>
                {
                    // Enable chaos in development and staging environments.
                    if (environment.IsDevelopment() || environment.IsStaging())
                    {
                        return ValueTask.FromResult(true);
                    }

                    // Enable chaos for specific users or tenants, even in production environments.
                    if (ShouldEnableChaos(args.Context))
                    {
                        return ValueTask.FromResult(true);
                    }

                    return ValueTask.FromResult(false);
                },
                InjectionRateGenerator = args =>
                {
                    if (environment.IsStaging())
                    {
                        // 1% chance of failure on staging environments.
                        return ValueTask.FromResult(0.01);
                    }

                    if (environment.IsDevelopment())
                    {
                        // 5% chance of failure on development environments.
                        return ValueTask.FromResult(0.05);
                    }

                    // The context can carry information to help determine the injection rate.
                    // For instance, in production environments, you might have certain test users or tenants
                    // for whom you wish to inject chaos.
                    if (ResolveInjectionRate(args.Context, out double injectionRate))
                    {
                        return ValueTask.FromResult(injectionRate);
                    }

                    // No chaos on production environments.
                    return ValueTask.FromResult(0.0);
                },
                FaultGenerator = new FaultGenerator()
                    .AddException<TimeoutException>()
                    .AddException<HttpRequestException>()
            });
        });

        #endregion
    }

    public static void ApplyChaosSelectivelyWithChaosManager(IServiceCollection services)
    {
        #region chaos-selective-manager

        services.AddResiliencePipeline("chaos-pipeline", (builder, context) =>
        {
            var chaosManager = context.ServiceProvider.GetRequiredService<IChaosManager>();

            builder
                .AddChaosFault(new ChaosFaultStrategyOptions
                {
                    EnabledGenerator = args => ValueTask.FromResult(chaosManager.IsChaosEnabled(args.Context)),
                    InjectionRateGenerator = args => ValueTask.FromResult(chaosManager.GetInjectionRate(args.Context)),
                    FaultGenerator = new FaultGenerator()
                        .AddException<TimeoutException>()
                        .AddException<HttpRequestException>()
                })
                .AddChaosLatency(new ChaosLatencyStrategyOptions
                {
                    EnabledGenerator = args => ValueTask.FromResult(chaosManager.IsChaosEnabled(args.Context)),
                    InjectionRateGenerator = args => ValueTask.FromResult(chaosManager.GetInjectionRate(args.Context)),
                    Latency = TimeSpan.FromSeconds(60)
                });
        });

        #endregion
    }

    private static bool ResolveInjectionRate(ResilienceContext context, out double injectionRate)
    {
        injectionRate = 0.0;
        return false;
    }

    private static bool ShouldEnableChaos(ResilienceContext context) => true;

    private static ValueTask RestartRedisAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    #region  chaos-manager

    public interface IChaosManager
    {
        bool IsChaosEnabled(ResilienceContext context);

        double GetInjectionRate(ResilienceContext context);
    }

    #endregion
}
