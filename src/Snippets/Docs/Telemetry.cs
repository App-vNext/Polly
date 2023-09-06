using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Telemetry;

namespace Snippets.Docs;

internal static class Telemetry
{
    public static void TelemetryCoordinates()
    {
        #region telemetry-coordinates

        var builder = new ResiliencePipelineBuilder();
        builder.Name = "my-name";
        builder.Name = "my-instance-name";

        builder.AddRetry(new RetryStrategyOptions
        {
            // The default value is "Retry"
            Name = "my-retry-name"
        });

        #endregion
    }

    public static void ConfigureTelemetry()
    {
        #region configure-telemetry

        var telemetryOptions = new TelemetryOptions
        {
            // Configure logging
            LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole())
        };

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

    #region telemetry-listeners

    internal class MyTelemetryListener : TelemetryListener
    {
        public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
        {
            Console.WriteLine($"Telemetry event occurred: {args.Event.EventName}");
        }
    }

    internal class MyMeteringEnricher : MeteringEnricher
    {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
        {
            context.Tags.Add(new("my-custom-tag", "custom-value"));
        }
    }

    #endregion
}
