using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace Snippets.Docs;

internal static class Readme
{
    public static async Task QuickStart()
    {
        #region quick-start

        // Create a instance of builder that exposes various extensions for adding resilience strategies
        var builder = new ResiliencePipelineBuilder();

        // Add retry using the default options:
        // - 3 retry attempts
        // - 1-second delay between retries
        // - Handles all exceptions except OperationCanceledException
        builder.AddRetry(new RetryStrategyOptions());

        // Add 10 second timeout
        builder.AddTimeout(TimeSpan.FromSeconds(10));

        // Build the resilience pipeline
        ResiliencePipeline pipeline = builder.Build();

        // Execute the pipeline
        await pipeline.ExecuteAsync(async token =>
        {
            // Your custom logic here
        });

        #endregion
    }

    public static async Task QuickStartDi()
    {
        #region quick-start-di

        var services = new ServiceCollection();

        // Define a resilience pipeline with the name "my-pipeline"
        services.AddResiliencePipeline("my-pipeline", builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions())
                .AddTimeout(TimeSpan.FromSeconds(10));
        });

        // Build the service provider
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Retrieve ResiliencePipelineProvider that caches and dynamically creates the resilience pipelines
        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

        // Retrieve resilience pipeline using the name it was registered with
        ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

        // Execute the pipeline
        await pipeline.ExecuteAsync(async token =>
        {
            // Your custom logic here
        });

        #endregion
    }
}
