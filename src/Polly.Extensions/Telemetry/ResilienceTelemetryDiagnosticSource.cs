using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Utils;
using Polly.Telemetry;

namespace Polly.Extensions.Telemetry;

internal class ResilienceTelemetryDiagnosticSource : DiagnosticSource
{
    internal static readonly Meter Meter = new(TelemetryUtil.PollyDiagnosticSource, "1.0");

    private readonly ILogger _logger;
    private readonly Func<ResilienceContext, object?, object?> _resultFormatter;
    private readonly List<Action<EnrichmentContext>> _enrichers;

    public ResilienceTelemetryDiagnosticSource(TelemetryOptions options)
    {
        _enrichers = options.Enrichers.ToList();
        _logger = options.LoggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        _resultFormatter = options.ResultFormatter;

        Counter = Meter.CreateCounter<int>(
            "resilience-events",
            description: "Tracks the number of resilience events that occurred in resilience strategies.");
    }

    public Counter<int> Counter { get; }

    public override bool IsEnabled(string name) => true;

    public override void Write(string name, object? value)
    {
        if (value is not TelemetryEventArguments args)
        {
            return;
        }

        LogEvent(args);
        MeterEvent(args);
    }

    private void MeterEvent(TelemetryEventArguments args)
    {
        if (!Counter.Enabled)
        {
            return;
        }

        var source = args.Source;

        var enrichmentContext = EnrichmentContext.Get(args.Context, args.Arguments, args.Outcome);
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.EventName, args.EventName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.BuilderName, source.BuilderName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyName, source.StrategyName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyType, source.StrategyType));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyKey, source.BuilderProperties.GetValue(TelemetryUtil.StrategyKey, null!)));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ResultType, args.Context.GetResultType()));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExceptionName, args.Outcome?.Exception?.GetType().FullName));
        EnrichmentUtil.Enrich(enrichmentContext, _enrichers);

        Counter.Add(1, enrichmentContext.TagsSpan);

        EnrichmentContext.Return(enrichmentContext);
    }

    private void LogEvent(TelemetryEventArguments args)
    {
        var strategyKey = args.Source.BuilderProperties.GetValue(TelemetryUtil.StrategyKey, null!);

        if (args.Outcome?.Exception is Exception exception)
        {
            Log.ResilienceEvent(_logger, args.EventName, args.Source.BuilderName, args.Source.StrategyName, args.Source.StrategyType, strategyKey, exception.Message, exception);
        }
        else
        {
            var result = args.Outcome?.Result;
            if (result is not null)
            {
                result = _resultFormatter(args.Context, result);
            }

            Log.ResilienceEvent(_logger, args.EventName, args.Source.BuilderName, args.Source.StrategyName, args.Source.StrategyType, strategyKey, result, null);
        }
    }
}
