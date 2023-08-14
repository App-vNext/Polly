using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

internal sealed class TelemetryListenerImpl : TelemetryListener
{
    internal static readonly Meter Meter = new(TelemetryUtil.PollyDiagnosticSource, "1.0");

    private readonly ILogger _logger;
    private readonly Func<ResilienceContext, object?, object?> _resultFormatter;
    private readonly List<MeteringEnricher> _enrichers;
    private readonly TelemetryListener? _listener;

    public TelemetryListenerImpl(TelemetryOptions options)
    {
        _enrichers = options.MeteringEnrichers.ToList();
        _logger = options.LoggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        _resultFormatter = options.ResultFormatter;
        _listener = options.TelemetryListener;

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

    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        _listener?.Write(in args);

        LogEvent(in args);
        MeterEvent(in args);
    }

    private static bool GetArgs<T, TArgs>(T inArgs, out TArgs outArgs)
    {
        if (typeof(T) == typeof(TArgs))
        {
            outArgs = Unsafe.As<T, TArgs>(ref inArgs);
            return true;
        }

        outArgs = default!;
        return false;
    }

    private static void AddCommonTags<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        var source = context.TelemetryEvent.Source;
        var ev = context.TelemetryEvent.Event;

        context.Tags.Add(new(ResilienceTelemetryTags.EventName, context.TelemetryEvent.Event.EventName));
        context.Tags.Add(new(ResilienceTelemetryTags.EventSeverity, context.TelemetryEvent.Event.Severity.AsString()));

        if (source.PipelineName is not null)
        {
            context.Tags.Add(new(ResilienceTelemetryTags.PipelineName, source.PipelineName));
        }

        if (source.PipelineInstanceName is not null)
        {
            context.Tags.Add(new(ResilienceTelemetryTags.PipelineInstance, source.PipelineInstanceName));
        }

        if (source.StrategyName is not null)
        {
            context.Tags.Add(new(ResilienceTelemetryTags.StrategyName, source.StrategyName));
        }

        if (context.TelemetryEvent.Context.OperationKey is not null)
        {
            context.Tags.Add(new(ResilienceTelemetryTags.OperationKey, context.TelemetryEvent.Context.OperationKey));
        }

        context.Tags.Add(new(ResilienceTelemetryTags.ResultType, context.TelemetryEvent.Context.GetResultType()));

        if (context.TelemetryEvent.Outcome?.Exception is Exception e)
        {
            context.Tags.Add(new(ResilienceTelemetryTags.ExceptionName, e.GetType().FullName));
        }
    }

    private void MeterEvent<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        var arguments = args.Arguments;

        if (GetArgs<TArgs, PipelineExecutedArguments>(args.Arguments, out var executionFinished))
        {
            if (!ExecutionDuration.Enabled)
            {
                return;
            }

            var tags = TagsList.Get();
            var context = new EnrichmentContext<TResult, TArgs>(in args, tags.Tags);
            UpdateEnrichmentContext(in context);
            tags.Tags.Add(new(ResilienceTelemetryTags.ExecutionHealth, args.Context.GetExecutionHealth()));
            ExecutionDuration.Record(executionFinished.Duration.TotalMilliseconds, tags.TagsSpan);
            TagsList.Return(tags);
        }
        else if (GetArgs<TArgs, ExecutionAttemptArguments>(args.Arguments, out var executionAttempt))
        {
            if (!AttemptDuration.Enabled)
            {
                return;
            }

            var tags = TagsList.Get();
            var context = new EnrichmentContext<TResult, TArgs>(in args, tags.Tags);
            UpdateEnrichmentContext(in context);
            context.Tags.Add(new(ResilienceTelemetryTags.AttemptNumber, executionAttempt.AttemptNumber.AsBoxedInt()));
            context.Tags.Add(new(ResilienceTelemetryTags.AttemptHandled, executionAttempt.Handled.AsBoxedBool()));
            AttemptDuration.Record(executionAttempt.Duration.TotalMilliseconds, tags.TagsSpan);
            TagsList.Return(tags);
        }
        else if (Counter.Enabled)
        {
            var tags = TagsList.Get();
            var context = new EnrichmentContext<TResult, TArgs>(in args, tags.Tags);
            UpdateEnrichmentContext(in context);
            Counter.Add(1, tags.TagsSpan);
            TagsList.Return(tags);
        }
    }

    private void UpdateEnrichmentContext<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        AddCommonTags(in context);

        if (_enrichers.Count != 0)
        {
            foreach (var enricher in _enrichers)
            {
                enricher.Enrich(in context);
            }
        }
    }

    private void LogEvent<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        var result = GetResult(args.Context, args.Outcome);
        var level = args.Event.Severity.AsLogLevel();

        if (GetArgs<TArgs, PipelineExecutingArguments>(args.Arguments, out _))
        {
            _logger.PipelineExecuting(
                args.Source.PipelineName.GetValueOrPlaceholder(),
                args.Source.PipelineInstanceName.GetValueOrPlaceholder(),
                args.Context.OperationKey,
                args.Context.GetResultType());
        }
        else if (GetArgs<TArgs, PipelineExecutedArguments>(args.Arguments, out var pipelineExecuted))
        {
            var logLevel = args.Context.IsExecutionHealthy() ? LogLevel.Debug : LogLevel.Warning;

            _logger.PipelineExecuted(
                logLevel,
                args.Source.PipelineName.GetValueOrPlaceholder(),
                args.Source.PipelineInstanceName.GetValueOrPlaceholder(),
                args.Context.OperationKey,
                args.Context.GetResultType(),
                GetResult(args.Context, args.Outcome),
                args.Context.GetExecutionHealth(),
                pipelineExecuted.Duration.TotalMilliseconds,
                args.Outcome?.Exception);
        }
        else if (GetArgs<TArgs, ExecutionAttemptArguments>(args.Arguments, out var executionAttempt))
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

    private object? GetResult<T>(ResilienceContext context, Outcome<T>? outcome)
    {
        if (outcome == null)
        {
            return null;
        }

        if (outcome.Value.Exception is not null)
        {
            return outcome.Value.Exception.Message;
        }

        return _resultFormatter(context, outcome.Value.Result);
    }
}
