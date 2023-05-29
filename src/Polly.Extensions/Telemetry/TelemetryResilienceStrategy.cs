using System;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly.Strategy;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Extensions.Telemetry;

internal sealed class TelemetryResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly string _builderName;
    private readonly string? _strategyKey;
    private readonly List<Action<EnrichmentContext>> _enrichers;
    private readonly ILogger _logger;

    // Temporary only, until the TimeProvider is exposed
    public TelemetryResilienceStrategy(
        string builderName,
        string? strategyKey,
        ILoggerFactory loggerFactory,
        List<Action<EnrichmentContext>> enrichers)
        : this(TimeProvider.System, builderName, strategyKey, loggerFactory, enrichers)
    {
    }

    public TelemetryResilienceStrategy(
        TimeProvider timeProvider,
        string builderName,
        string? strategyKey,
        ILoggerFactory loggerFactory,
        List<Action<EnrichmentContext>> enrichers)
    {
        _timeProvider = timeProvider;
        _builderName = builderName;
        _strategyKey = strategyKey;
        _enrichers = enrichers;
        _logger = loggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        ExecutionDuration = ResilienceTelemetryDiagnosticSource.Meter.CreateHistogram<double>(
            "strategy-execution-duration",
            unit: "ms",
            description: "The execution duration and execution result of resilience strategies.");
    }

    public Histogram<double> ExecutionDuration { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var stamp = _timeProvider.GetTimestamp();
        Log.ExecutingStrategy(_logger, _builderName, _strategyKey, context.GetResultType());

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        var duration = _timeProvider.GetElapsedTime(stamp);
        Log.StrategyExecuted(
            _logger,
            _builderName,
            _strategyKey,
            context.GetResultType(),
            ExpandOutcome(outcome),
            context.GetExecutionHealth(),
            duration.TotalMilliseconds,
            outcome.Exception);

        var tags = new TagList
            {
                { ResilienceTelemetryTags.BuilderName, _builderName },
                { ResilienceTelemetryTags.StrategyKey, _strategyKey },
                { ResilienceTelemetryTags.ResultType, context.GetResultType() },
                { ResilienceTelemetryTags.ExceptionName, outcome.Exception?.GetType().FullName },
                { ResilienceTelemetryTags.ExecutionHealth, context.GetExecutionHealth() }
            };

        EnrichmentUtil.Enrich(ref tags, _enrichers, context, outcome.AsObjectOutcome(), resilienceArguments: null);

        ExecutionDuration.Record(duration.TotalMilliseconds, tags);

        return outcome;
    }

    private static object? ExpandOutcome<TResult>(Outcome<TResult> outcome)
    {
        // stryker disable once all: no means to test this
        return (object)outcome.Exception?.Message! ?? outcome.Result;
    }
}
