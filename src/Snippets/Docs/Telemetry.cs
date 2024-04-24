using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly.Telemetry;

namespace Snippets.Docs;

internal static class Telemetry
{
    public static void TelemetryCoordinates()
    {
        #region telemetry-tags

        var builder = new ResiliencePipelineBuilder();
        builder.Name = "my-name";
        builder.InstanceName = "my-instance-name";

        builder.AddRetry(new RetryStrategyOptions
        {
            // The default value is "Retry"
            Name = "my-retry-name"
        });

        ResiliencePipeline pipeline = builder.Build();

        // Create resilience context with operation key
        ResilienceContext resilienceContext = ResilienceContextPool.Shared.Get("my-operation-key");

        // Execute the pipeline with the context
        pipeline.Execute(
            context =>
            {
                // Your code comes here
            },
            resilienceContext);

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

    public static void MeteringEnricherRegistration()
    {
        #region metering-enricher-registration

        var telemetryOptions = new TelemetryOptions();

        // Register custom enricher
        telemetryOptions.MeteringEnrichers.Add(new CustomMeteringEnricher());

        var builder = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions())
            .ConfigureTelemetry(telemetryOptions) // This method enables telemetry in the builder
            .Build();

        #endregion
    }

    #region metering-enricher

    internal sealed class CustomMeteringEnricher : MeteringEnricher
    {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
        {
            // You can read additional details from any resilience event and use it to enrich the telemetry
            if (context.TelemetryEvent.Arguments is OnRetryArguments<TResult> retryArgs)
            {
                // See https://github.com/open-telemetry/semantic-conventions/blob/main/docs/general/metrics.md for more details
                // on how to name the tags.
                context.Tags.Add(new("retry.attempt", retryArgs.AttemptNumber));
            }
        }
    }

    #endregion

    public static void SeverityOverrides()
    {
        var services = new ServiceCollection();

        #region telemetry-severity-override

        services.AddResiliencePipeline("my-strategy", (builder, context) =>
        {
            // Create a new instance of telemetry options by using copy-constructor of the global ones.
            // This ensures that common configuration is preserved.
            var telemetryOptions = new TelemetryOptions(context.GetOptions<TelemetryOptions>());

            telemetryOptions.SeverityProvider = ev =>
            {
                if (ev.EventName == "OnRetry")
                {
                    // Decrease the severity of particular event.
                    return ResilienceEventSeverity.Debug;
                }

                return ev.Severity;
            };

            builder.AddTimeout(TimeSpan.FromSeconds(1));

            // Override the telemetry configuration for this pipeline.
            builder.ConfigureTelemetry(telemetryOptions);
        });

        #endregion
    }
}
