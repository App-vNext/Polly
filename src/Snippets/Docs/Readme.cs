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

        // Create a instance of builder that exposes various extensions for adding resilience strategies
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
            .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 second timeout
            .Build(); // Builds the resilience pipeline

        // Execute the pipeline asynchronously
        await pipeline.ExecuteAsync(static async cancellationToken => { /*Your custom logic here */ }, cancellationToken);

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
        await pipeline.ExecuteAsync(static async token =>
        {
            // Your custom logic here
        });

        #endregion
    }
}
