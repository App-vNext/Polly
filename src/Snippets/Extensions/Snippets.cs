using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Telemetry;

namespace Snippets.Extensions;

internal static class Snippets
{
    public static async Task AddResiliencePipeline()
    {
        #region add-resilience-pipeline

        var services = new ServiceCollection();

        // Define a resilience pipeline
        services.AddResiliencePipeline(
          "my-key",
          builder => builder.AddTimeout(TimeSpan.FromSeconds(10)));

        // Define a resilience pipeline with custom options
        services
            .Configure<MyTimeoutOptions>(options => options.Timeout = TimeSpan.FromSeconds(10))
            .AddResiliencePipeline(
                "my-timeout",
                (builder, context) =>
                {
                    var myOptions = context.GetOptions<MyTimeoutOptions>();

                    builder.AddTimeout(myOptions.Timeout);
                });

        // Resolve the resilience pipeline
        var serviceProvider = services.BuildServiceProvider();
        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var pipeline = pipelineProvider.GetPipeline("my-key");

        // Use it
        await pipeline.ExecuteAsync(async cancellation => await Task.Delay(100, cancellation));

        #endregion
    }

    public static void AddResiliencePipelineWithTelemetry()
    {
        #region add-resilience-pipeline-with-telemetry

        var serviceCollection = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .AddResiliencePipeline("my-strategy", builder => builder.AddTimeout(TimeSpan.FromSeconds(1)))
            .Configure<TelemetryOptions>(options =>
            {
                // Configure enrichers
                options.MeteringEnrichers.Add(new MyMeteringEnricher());

                // Configure telemetry listeners
                options.TelemetryListeners.Add(new MyTelemetryListener());
            });
        #endregion
    }

    private class MyTimeoutOptions
    {
        public TimeSpan Timeout { get; set; }
    }
}
