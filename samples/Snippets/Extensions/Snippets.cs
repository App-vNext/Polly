using Microsoft.Extensions.Logging;
using Polly.Telemetry;
using Polly;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Snippets.Extensions;

#pragma warning disable IDE0022 // Use expression body for method

internal static class Snippets
{
    public static void ConfigureTelemetry()
    {
        #region configure-telemetry

        var telemetryOptions = new TelemetryOptions();

        // Configure logging
        telemetryOptions.LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        // Configure enrichers
        telemetryOptions.MeteringEnrichers.Add(new MyMeteringEnricher());

        // Configure telemetry listeners
        telemetryOptions.TelemetryListeners.Add(new MyTelemetryListener());

        var builder = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .ConfigureTelemetry(telemetryOptions) // This method enables telemetry in the builder
            .Build();

        #endregion
    }

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
        await pipeline.ExecuteAsync(async cancellation => await Task.Delay(100));

        #endregion
    }

    public static void AddResiliencePipelineWithTelemetry()
    {
        #region add-resilience-pipeline-with-telemetry

        var serviceCollection = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .AddResiliencePipeline("my-strategy", builder => builder.AddTimeout(TimeSpan.FromSeconds(1)))
            // Configure the default settings for TelemetryOptions
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

    #region telemetry-listeners

    class MyTelemetryListener : TelemetryListener
    {
        public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
        {
            Console.WriteLine($"Telemetry event occurred: {args.Event.EventName}");
        }
    }

    class MyMeteringEnricher : MeteringEnricher
    {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
        {
            context.Tags.Add(new("my-custom-tag", "custom-value"));
        }
    }

    #endregion
}
