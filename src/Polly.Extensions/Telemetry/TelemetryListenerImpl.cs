using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

internal sealed class TelemetryListenerImpl : TelemetryListener
{
    internal static readonly Meter Meter = new(TelemetryUtil.PollyDiagnosticSource, "1.0");

    private readonly ILogger _logger;
    private readonly Func<ResilienceContext, object?, object?> _resultFormatter;
    private readonly List<TelemetryListener> _listeners;
    private readonly Func<SeverityProviderArguments, ResilienceEventSeverity>? _severityProvider;
    private readonly List<MeteringEnricher> _enrichers;

    public TelemetryListenerImpl(TelemetryOptions options)
    {
        _enrichers = [.. options.MeteringEnrichers];
        _logger = options.LoggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        _resultFormatter = options.ResultFormatter;
        _listeners = [.. options.TelemetryListeners];
        _severityProvider = options.SeverityProvider;

        Counter = Meter.CreateCounter<int>(
            "resilience.polly.strategy.events",
            description: "Tracks the number of resilience events that occurred in resilience strategies.");

        AttemptDuration = Meter.CreateHistogram<double>(
            "resilience.polly.strategy.attempt.duration",
            unit: "ms",
            description: "Tracks the duration of execution attempts.");

        ExecutionDuration = Meter.CreateHistogram<double>(
            "resilience.polly.pipeline.duration",
            unit: "ms",
            description: "The execution duration of resilience pipelines.");
    }

    public Counter<int> Counter { get; }

    public Histogram<double> AttemptDuration { get; }

    public Histogram<double> ExecutionDuration { get; }

    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        // stryker disable once equality : no means to test this
        if (_listeners.Count > 0)
        {
            foreach (var listener in _listeners)
            {
                listener.Write(in args);
            }
        }

        var severity = _severityProvider is { } provider
            ? provider(new SeverityProviderArguments(args.Source, args.Event, args.Context))
            : args.Event.Severity;

        LogEvent(in args, severity);
        MeterEvent(in args, severity);
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

    private static void AddCommonTags<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context, ResilienceEventSeverity severity)
    {
        var source = context.TelemetryEvent.Source;
        var ev = context.TelemetryEvent.Event;

        context.Tags.Add(new(ResilienceTelemetryTags.EventName, context.TelemetryEvent.Event.EventName));
        context.Tags.Add(new(ResilienceTelemetryTags.EventSeverity, severity.AsString()));

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

        if (context.TelemetryEvent.Outcome?.Exception is Exception e)
        {
            context.Tags.Add(new(ResilienceTelemetryTags.ExceptionType, e.GetType().FullName));
        }
    }

    private void MeterEvent<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args, ResilienceEventSeverity severity)
    {
        var arguments = args.Arguments;

        if (GetArgs<TArgs, PipelineExecutedArguments>(args.Arguments, out var executionFinished))
        {
            if (ExecutionDuration.Enabled)
            {
                var tags = TagsList.Get();
                var context = new EnrichmentContext<TResult, TArgs>(in args, tags);
                UpdateEnrichmentContext(in context, severity);
                ExecutionDuration.Record(executionFinished.Duration.TotalMilliseconds, tags.TagsSpan);
                TagsList.Return(tags);
            }
        }
        else if (GetArgs<TArgs, ExecutionAttemptArguments>(args.Arguments, out var executionAttempt))
        {
            if (AttemptDuration.Enabled)
            {
                var tags = TagsList.Get();
                var context = new EnrichmentContext<TResult, TArgs>(in args, tags);
                UpdateEnrichmentContext(in context, severity);
                context.Tags.Add(new(ResilienceTelemetryTags.AttemptNumber, executionAttempt.AttemptNumber.AsBoxedInt()));
                context.Tags.Add(new(ResilienceTelemetryTags.AttemptHandled, executionAttempt.Handled.AsBoxedBool()));
                AttemptDuration.Record(executionAttempt.Duration.TotalMilliseconds, tags.TagsSpan);
                TagsList.Return(tags);
            }
        }
        else if (Counter.Enabled)
        {
            var tags = TagsList.Get();
            var context = new EnrichmentContext<TResult, TArgs>(in args, tags);
            UpdateEnrichmentContext(in context, severity);
            Counter.Add(1, tags.TagsSpan);
            TagsList.Return(tags);
        }
    }

    private void UpdateEnrichmentContext<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context, ResilienceEventSeverity severity)
    {
        AddCommonTags(in context, severity);

        if (_enrichers.Count != 0)
        {
            foreach (var enricher in _enrichers)
            {
                enricher.Enrich(in context);
            }
        }
    }

    private void LogEvent<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args, ResilienceEventSeverity severity)
    {
        var result = GetResult(args.Context, args.Outcome);
        var level = severity.AsLogLevel();
        var pipelineName = args.Source.PipelineName.GetValueOrPlaceholder();
        var pipelineInstanceName = args.Source.PipelineInstanceName.GetValueOrPlaceholder();
        var strategyName = args.Source.StrategyName.GetValueOrPlaceholder();
        var operationKey = args.Context.OperationKey;
        var exception = args.Outcome?.Exception;

        if (GetArgs<TArgs, PipelineExecutingArguments>(args.Arguments, out _))
        {
            _logger.PipelineExecuting(
                level,
                pipelineName,
                pipelineInstanceName,
                operationKey);
        }
        else if (GetArgs<TArgs, PipelineExecutedArguments>(args.Arguments, out var pipelineExecuted))
        {
            _logger.PipelineExecuted(
                level,
                pipelineName,
                pipelineInstanceName,
                operationKey,
                result,
                pipelineExecuted.Duration.TotalMilliseconds,
                exception);
        }
        else if (GetArgs<TArgs, ExecutionAttemptArguments>(args.Arguments, out var executionAttempt))
        {
            _logger.ExecutionAttempt(
                level,
                pipelineName,
                pipelineInstanceName,
                strategyName,
                operationKey,
                result,
                executionAttempt.Handled,
                executionAttempt.AttemptNumber,
                executionAttempt.Duration.TotalMilliseconds,
                exception);
        }
        else
        {
            _logger.ResilienceEvent(
                level,
                args.Event.EventName,
                pipelineName,
                pipelineInstanceName,
                strategyName,
                operationKey,
                result,
                exception);
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
