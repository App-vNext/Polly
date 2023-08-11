using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

internal sealed class ResilienceTelemetryDiagnosticSource : DiagnosticSource
{
    internal static readonly Meter Meter = new(TelemetryUtil.PollyDiagnosticSource, "1.0");

    private readonly ILogger _logger;
    private readonly Func<ResilienceContext, object?, object?> _resultFormatter;
    private readonly Action<TelemetryEventArguments>? _onEvent;
    private readonly List<Action<EnrichmentContext>> _enrichers;

    public ResilienceTelemetryDiagnosticSource(TelemetryOptions options)
    {
        _enrichers = options.Enrichers.ToList();
        _logger = options.LoggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        _resultFormatter = options.ResultFormatter;
        _onEvent = options.OnTelemetryEvent;

        Counter = Meter.CreateCounter<int>(
            "resilience-events",
            description: "Tracks the number of resilience events that occurred in resilience strategies.");

        AttemptDuration = Meter.CreateHistogram<double>(
            "execution-attempt-duration",
            unit: "ms",
            description: "Tracks the duration of execution attempts.");

        ExecutionDuration = Meter.CreateHistogram<double>(
            "pipeline-execution-duration",
            unit: "ms",
            description: "The execution duration and execution results of resilience pipelines.");
    }

    public Counter<int> Counter { get; }

    public Histogram<double> AttemptDuration { get; }

    public Histogram<double> ExecutionDuration { get; }

    public override bool IsEnabled(string name) => true;

#pragma warning disable IL2046 // 'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.
#pragma warning disable IL3051 // 'RequiresDynamicCodeAttribute' annotations must match across all interface implementations or overrides.
    public override void Write(string name, object? value)
#pragma warning restore IL3051 // 'RequiresDynamicCodeAttribute' annotations must match across all interface implementations or overrides.
#pragma warning restore IL2046 // 'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.
    {
        if (value is not TelemetryEventArguments args)
        {
            return;
        }

        _onEvent?.Invoke(args);

        LogEvent(args);
        MeterEvent(args);
    }

    private static void AddCommonTags(TelemetryEventArguments args, ResilienceTelemetrySource source, EnrichmentContext enrichmentContext)
    {
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.EventName, args.Event.EventName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.EventSeverity, args.Event.Severity.AsString()));

        if (source.PipelineName is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.PipelineName, source.PipelineName));
        }

        if (source.PipelineInstanceName is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.PipelineInstance, source.PipelineInstanceName));
        }

        if (source.StrategyName is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyName, source.StrategyName));
        }

        if (enrichmentContext.Context.OperationKey is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.OperationKey, enrichmentContext.Context.OperationKey));
        }

        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ResultType, args.Context.GetResultType()));

        if (args.Outcome?.Exception is Exception e)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExceptionName, e.GetType().FullName));
        }
    }

    private void MeterEvent(TelemetryEventArguments args)
    {
        var source = args.Source;

        if (args.Arguments is PipelineExecutedArguments executionFinishedArguments)
        {
            if (!ExecutionDuration.Enabled)
            {
                return;
            }

            var enrichmentContext = EnrichmentContext.Get(args.Context, null, args.Outcome);
            AddCommonTags(args, source, enrichmentContext);
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExecutionHealth, args.Context.GetExecutionHealth()));
            EnrichmentUtil.Enrich(enrichmentContext, _enrichers);

            ExecutionDuration.Record(executionFinishedArguments.Duration.TotalMilliseconds, enrichmentContext.TagsSpan);
            EnrichmentContext.Return(enrichmentContext);
        }
        else if (args.Arguments is ExecutionAttemptArguments executionAttempt)
        {
            if (!AttemptDuration.Enabled)
            {
                return;
            }

            var enrichmentContext = EnrichmentContext.Get(args.Context, args.Arguments, args.Outcome);
            AddCommonTags(args, source, enrichmentContext);
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.AttemptNumber, executionAttempt.AttemptNumber.AsBoxedInt()));
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.AttemptHandled, executionAttempt.Handled.AsBoxedBool()));
            EnrichmentUtil.Enrich(enrichmentContext, _enrichers);
            AttemptDuration.Record(executionAttempt.Duration.TotalMilliseconds, enrichmentContext.TagsSpan);
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
        var result = args.Outcome?.Result;
        if (result is not null)
        {
            result = _resultFormatter(args.Context, result);
        }
        else if (args.Outcome?.Exception is Exception e)
        {
            result = e.Message;
        }

        var level = args.Event.Severity.AsLogLevel();

        if (args.Arguments is PipelineExecutingArguments pipelineExecutionStarted)
        {
            _logger.PipelineExecuting(
                args.Source.PipelineName.GetValueOrPlaceholder(),
                args.Source.PipelineInstanceName.GetValueOrPlaceholder(),
                args.Context.OperationKey,
                args.Context.GetResultType());
        }
        else if (args.Arguments is PipelineExecutedArguments pipelineExecutionFinished)
        {
            var logLevel = args.Context.IsExecutionHealthy() ? LogLevel.Debug : LogLevel.Warning;

            _logger.PipelineExecuted(
                logLevel,
                args.Source.PipelineName.GetValueOrPlaceholder(),
                args.Source.PipelineInstanceName.GetValueOrPlaceholder(),
                args.Context.OperationKey,
                args.Context.GetResultType(),
                ExpandOutcome(args.Context, args.Outcome),
                args.Context.GetExecutionHealth(),
                pipelineExecutionFinished.Duration.TotalMilliseconds,
                args.Outcome?.Exception);
        }
        else if (args.Arguments is ExecutionAttemptArguments executionAttempt)
        {
            if (_logger.IsEnabled(level))
            {
                _logger.ExecutionAttempt(
                    level,
                    args.Source.PipelineName.GetValueOrPlaceholder(),
                    args.Source.PipelineInstanceName.GetValueOrPlaceholder(),
                    args.Source.StrategyName.GetValueOrPlaceholder(),
                    args.Context.OperationKey,
                    result,
                    executionAttempt.Handled,
                    executionAttempt.AttemptNumber,
                    executionAttempt.Duration.TotalMilliseconds,
                    args.Outcome?.Exception);
            }
        }
        else
        {
            _logger.ResilienceEvent(
                level,
                args.Event.EventName,
                args.Source.PipelineName.GetValueOrPlaceholder(),
                args.Source.PipelineInstanceName.GetValueOrPlaceholder(),
                args.Source.StrategyName.GetValueOrPlaceholder(),
                args.Context.OperationKey,
                result,
                args.Outcome?.Exception);
        }
    }

    private object? ExpandOutcome(ResilienceContext context, Outcome<object>? outcome)
    {
        if (outcome == null)
        {
            return null;
        }

        // stryker disable once all: no means to test this
        return (object)outcome.Value.Exception?.Message! ?? _resultFormatter(context, outcome.Value.Result);
    }
}
