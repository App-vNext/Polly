using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.CircuitBreaker;
using Polly.Registry;
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

        // 2% of all requests will be injected with chaos fault.
        const double faultInjectionRate = 0.02;
        // For the remaining 98% of total requests, 50% of them will be injected with latency. Then 49% of total request will be injected with chaos latency.
        // Latency injection does not return early.
        const double latencyInjectionRate = 0.50;
        // For the remaining 98% of total requests, 10% of them will be injected with outcome. Then 9.8% of total request will be injected with chaos outcome.
        const double outcomeInjectionRate = 0.10;
        // For the remaining 88.2% of total requests, 1% of them will be injected with behavior. Then 0.882% of total request will be injected with chaos behavior.
        const double behaviorInjectionRate = 0.01;

        builder
            .AddChaosFault(faultInjectionRate, () => new InvalidOperationException("Injected by chaos strategy!")) // Inject a chaos fault to executions
            .AddChaosLatency(latencyInjectionRate, TimeSpan.FromMinutes(1)) // Inject a chaos latency to executions
            .AddChaosOutcome(outcomeInjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)) // Inject a chaos outcome to executions
            .AddChaosBehavior(behaviorInjectionRate, cancellationToken => RestartRedisAsync(cancellationToken)); // Inject a chaos behavior to executions

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
                    EnabledGenerator = args => chaosManager.IsChaosEnabled(args.Context),
                    InjectionRateGenerator = args => chaosManager.GetInjectionRate(args.Context),
                    FaultGenerator = new FaultGenerator()
                        .AddException<TimeoutException>()
                        .AddException<HttpRequestException>()
                })
                .AddChaosLatency(new ChaosLatencyStrategyOptions
                {
                    EnabledGenerator = args => chaosManager.IsChaosEnabled(args.Context),
                    InjectionRateGenerator = args => chaosManager.GetInjectionRate(args.Context),
                    Latency = TimeSpan.FromSeconds(60)
                });
        });

        #endregion
    }

    public static void CentralPipeline(IServiceCollection services)
    {
        #region chaos-central-pipeline

        services.AddResiliencePipeline("chaos-pipeline", (builder, context) =>
        {
            var chaosManager = context.ServiceProvider.GetRequiredService<IChaosManager>();

            builder
                .AddChaosFault(new ChaosFaultStrategyOptions
                {
                    FaultGenerator = new FaultGenerator()
                        .AddException<TimeoutException>()
                        .AddException<HttpRequestException>()
                })
                .AddChaosLatency(new ChaosLatencyStrategyOptions
                {
                    Latency = TimeSpan.FromSeconds(60)
                });
        });

        #endregion
    }

    public static void CentralPipelineIntegration(IServiceCollection services)
    {
        #region chaos-central-pipeline-integration

        services.AddResiliencePipeline("my-pipeline-1", (builder, context) =>
        {
            var pipelineProvider = context.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
            var chaosPipeline = pipelineProvider.GetPipeline("chaos-pipeline");

            builder
                .AddRetry(new RetryStrategyOptions())
                .AddTimeout(TimeSpan.FromSeconds(5))
                .AddPipeline(chaosPipeline); // Inject central chaos pipeline

        });

        #endregion
    }

    #region chaos-extension

    // Options that represent the chaos pipeline
    public sealed class MyChaosOptions
    {
        public ChaosFaultStrategyOptions Fault { get; set; } = new()
        {
            FaultGenerator = new FaultGenerator()
                .AddException<TimeoutException>()
                .AddException<HttpRequestException>()
        };

        public ChaosLatencyStrategyOptions Latency { get; set; } = new()
        {
            Latency = TimeSpan.FromSeconds(60)
        };
    }

    // Extension for easy integration of the chaos pipeline
    public static void AddMyChaos(this ResiliencePipelineBuilder builder, Action<MyChaosOptions>? configure = null)
    {
        var options = new MyChaosOptions();
        configure?.Invoke(options);

        builder
            .AddChaosFault(options.Fault)
            .AddChaosLatency(options.Latency);
    }

    #endregion

    public static void ExtensionIntegration(IServiceCollection services)
    {
        #region chaos-extension-integration

        services.AddResiliencePipeline("my-pipeline-1", (builder, context) =>
        {
            builder
                .AddRetry(new RetryStrategyOptions())
                .AddTimeout(TimeSpan.FromSeconds(5))
                .AddMyChaos(); // Use the extension
        });

        services.AddResiliencePipeline("my-pipeline-2", (builder, context) =>
        {
            builder
                .AddRetry(new RetryStrategyOptions())
                .AddTimeout(TimeSpan.FromSeconds(5))
                .AddMyChaos(options =>
                {
                    options.Latency.InjectionRate = 0.1; // Override the default injection rate
                    options.Latency.Latency = TimeSpan.FromSeconds(10); // Override the default latency
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
        ValueTask<bool> IsChaosEnabled(ResilienceContext context);

        ValueTask<double> GetInjectionRate(ResilienceContext context);
    }

    #endregion
}
