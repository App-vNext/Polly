using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
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

        AttemptDuration = Meter.CreateHistogram<double>(
            "execution-attempt-duration",
            unit: "ms",
            description: "Tracks the duration of execution attempts.");
    }

    public Counter<int> Counter { get; }

    public Histogram<double> AttemptDuration { get; }

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

    private static void AddCommonTags(TelemetryEventArguments args, ResilienceTelemetrySource source, EnrichmentContext enrichmentContext)
    {
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.EventName, args.EventName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.BuilderName, source.BuilderName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyName, source.StrategyName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyType, source.StrategyType));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyKey, source.BuilderProperties.GetValue(TelemetryUtil.StrategyKey, null!)));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ResultType, args.Context.GetResultType()));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExceptionName, args.Outcome?.Exception?.GetType().FullName));
    }

    private void MeterEvent(TelemetryEventArguments args)
    {
        var source = args.Source;

        if (args.Arguments is ExecutionAttemptArguments executionAttempt)
        {
            if (!AttemptDuration.Enabled)
            {
                return;
            }

            var enrichmentContext = EnrichmentContext.Get(args.Context, args.Arguments, args.Outcome);
            AddCommonTags(args, source, enrichmentContext);
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.AttemptNumber, executionAttempt.Attempt.AsBoxedInt()));
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.AttemptHandled, executionAttempt.Handled.AsBoxedBool()));
            EnrichmentUtil.Enrich(enrichmentContext, _enrichers);
            AttemptDuration.Record(executionAttempt.ExecutionTime.TotalMilliseconds, enrichmentContext.TagsSpan);
            EnrichmentContext.Return(enrichmentContext);
        }
        else if (Counter.Enabled)
        {
            var enrichmentContext = EnrichmentContext.Get(args.Context, args.Arguments, args.Outcome);
            AddCommonTags(args, source, enrichmentContext);
            EnrichmentUtil.Enrich(enrichmentContext, _enrichers);
            Counter.Add(1, enrichmentContext.TagsSpan);
            EnrichmentContext.Return(enrichmentContext);
        }
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
