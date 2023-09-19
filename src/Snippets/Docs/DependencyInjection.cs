﻿using System.Net.Http;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;

namespace Snippets.Docs;

internal static class DependencyInjection
{
    public static async Task AddResiliencePipeline()
    {
        #region add-resilience-pipeline

        var services = new ServiceCollection();

        // Define a resilience pipeline
        services.AddResiliencePipeline("my-key", builder =>
        {
            // Add strategies to your pipeline here, timeout for example
            builder.AddTimeout(TimeSpan.FromSeconds(10));
        });

        // You can also access IServiceProvider by using the alternate overload
        services.AddResiliencePipeline("my-key", (builder, context) =>
        {
            // Resolve any service from DI
            var loggerFactory = context.ServiceProvider.GetRequiredService<ILoggerFactory>();

            // Add strategies to your pipeline here
            builder.AddTimeout(TimeSpan.FromSeconds(10));
        });

        // Resolve the resilience pipeline
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ResiliencePipelineProvider<string> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-key");

        // Use it
        await pipeline.ExecuteAsync(async cancellation => await Task.Delay(100, cancellation));

        #endregion
    }

    public static async Task AddResiliencePipelineGeneric()
    {
        using var client = new HttpClient();
        var endpoint = new Uri("https://www.dummy.com");
        var cancellationToken = CancellationToken.None;

        #region add-resilience-pipeline-generic

        var services = new ServiceCollection();

        // Define a generic resilience pipeline
        // First parameter is the type of key, second one is the type of the results the generic pipeline works with
        services.AddResiliencePipeline<string, HttpResponseMessage>("my-pipeline", builder =>
        {
            builder.AddRetry(new()
            {
                MaxRetryAttempts = 2,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(response => response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            })
            .AddTimeout(TimeSpan.FromSeconds(2));
        });

        // Resolve the resilience pipeline
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ResiliencePipelineProvider<string> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        ResiliencePipeline<HttpResponseMessage> pipeline = pipelineProvider.GetPipeline<HttpResponseMessage>("my-key");

        // Use it
        await pipeline.ExecuteAsync(
            async cancellation => await client.GetAsync(endpoint, cancellation),
            cancellationToken);

        #endregion
    }

    public static async Task DynamicReloads(IServiceCollection services, IConfigurationSection configurationSection)
    {
        #region di-dynamic-reloads

        services
            .Configure<RetryStrategyOptions>("my-retry-options", configurationSection) // Configure the options
            .AddResiliencePipeline("my-pipeline", (builder, context) =>
            {
                // Enable the reloads whenever the named options change
                context.EnableReloads<RetryStrategyOptions>("my-retry-options");

                // Utility method to retrieve the named options
                var retryOptions = context.GetOptions<RetryStrategyOptions>("my-retry-options");

                // Add retries using the resolved options
                builder.AddRetry(retryOptions);
            });

        #endregion
    }

    public static async Task ResourceDisposal(IServiceCollection services)
    {
        #region di-resource-disposal

        services.AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            // Create disposable resource
            var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = 100, QueueLimit = 100 });

            // Use it
            builder.AddRateLimiter(limiter);

            // Dispose the resource created in the callback when the pipeline is discarded
            context.OnPipelineDisposed(() => limiter.Dispose());
        });

        #endregion
    }

    #region di-registry-complex-key

    public record struct MyPipelineKey(string PipelineName, string InstanceName)
    {
    }

    #endregion

    public static async Task AddResiliencePipelineWithComplexKey(IServiceCollection services)
    {
        #region di-registry-add-pipeline

        services.AddResiliencePipeline(new MyPipelineKey("my-pipeline", string.Empty), builder =>
        {
            // Circuit breaker is a stateful strategy. To isolate the builder across different pipelines,
            // we must use multiple instances.
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
        });

        #endregion
    }

    #region di-complex-key-comparer

    public sealed class PipelineNameComparer : IEqualityComparer<MyPipelineKey>
    {
        public bool Equals(MyPipelineKey x, MyPipelineKey y) => x.PipelineName == y.PipelineName;

        public int GetHashCode(MyPipelineKey obj) => (obj.PipelineName, obj.InstanceName).GetHashCode();
    }

    #endregion

    public static async Task ConfigureRegistry(IServiceCollection services)
    {
        #region di-registry-configure

        services
            .AddResiliencePipelineRegistry<MyPipelineKey>(options =>
            {
                options.BuilderComparer = new PipelineNameComparer();

                options.InstanceNameFormatter = key => key.InstanceName;

                options.BuilderNameFormatter = key => key.PipelineName;
            });

        #endregion
    }

    public static async Task ConfigureRegistry(IServiceProvider serviceProvider)
    {
        #region di-registry-multiple-instances

        ResiliencePipelineProvider<MyPipelineKey> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<MyPipelineKey>>();

        // The registry dynamically creates and caches instance-A using the associated builder action
        ResiliencePipeline instanceA = pipelineProvider.GetPipeline(new MyPipelineKey("my-pipeline", "instance-A"));

        // The registry creates and caches instance-B
        ResiliencePipeline instanceB = pipelineProvider.GetPipeline(new MyPipelineKey("my-pipeline", "instance-B"));

        #endregion
    }
}
