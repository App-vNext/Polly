using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Utils;
using Polly.Telemetry;

namespace Polly.Extensions.Telemetry;

internal class ResilienceTelemetryDiagnosticSource : DiagnosticSource
{
    internal static readonly Meter Meter = new(TelemetryUtil.PollyDiagnosticSource, "1.0");

    private readonly ILogger _logger;
    private readonly List<Action<EnrichmentContext>> _enrichers;

    public ResilienceTelemetryDiagnosticSource(TelemetryResilienceStrategyOptions options)
    {
        _enrichers = options.Enrichers.ToList();
        _logger = options.LoggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
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
        var source = args.Source;
        var tags = new TagList
        {
            { ResilienceTelemetryTags.EventName, args.EventName },
            { ResilienceTelemetryTags.BuilderName, source.BuilderName },
            { ResilienceTelemetryTags.StrategyName, source.StrategyName },
            { ResilienceTelemetryTags.StrategyType, source.StrategyType },
            { ResilienceTelemetryTags.StrategyKey, source.BuilderProperties.GetValue(TelemetryUtil.StrategyKey, null!) },
            { ResilienceTelemetryTags.ResultType, args.Context.GetResultType() },
            { ResilienceTelemetryTags.ExceptionName, args.Outcome?.Exception?.GetType().FullName }
        };

        EnrichmentUtil.Enrich(ref tags, _enrichers, args.Context, args.Outcome, args.Arguments);
        Counter.Add(1, tags);
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
            Log.ResilienceEvent(_logger, args.EventName, args.Source.BuilderName, args.Source.StrategyName, args.Source.StrategyType, strategyKey, args.Outcome?.Result, null);
        }
    }
}
