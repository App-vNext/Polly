using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;
using Polly.Retry;

namespace Snippets.Docs;

internal static class Readme
{
    public static async Task QuickStart()
    {
        CancellationToken cancellationToken = default;

        #region quick-start

        // Create an instance of builder that exposes various extensions for adding resilience strategies
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
            .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
            .Build(); // Builds the resilience pipeline

        // Execute the pipeline asynchronously
        await pipeline.ExecuteAsync(static async token => { /* Your custom logic goes here */ }, cancellationToken);

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
        var serviceProvider = services.BuildServiceProvider();

        // Retrieve a ResiliencePipelineProvider that dynamically creates and caches the resilience pipelines
        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

        // Retrieve your resilience pipeline using the name it was registered with
        ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

        // Alternatively, you can use keyed services to retrieve the resilience pipeline
        pipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");

        // Execute the pipeline
        await pipeline.ExecuteAsync(static async token =>
        {
            // Your custom logic goes here
        });

        #endregion
    }
}
