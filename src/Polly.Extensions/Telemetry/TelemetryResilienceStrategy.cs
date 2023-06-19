using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Extensions.Utils;
using Polly.Utils;

namespace Polly.Extensions.Telemetry;

internal sealed class TelemetryResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly string? _builderName;
    private readonly string? _strategyKey;
    private readonly List<Action<EnrichmentContext>> _enrichers;
    private readonly ILogger _logger;
    private readonly Func<ResilienceContext, object?, object?> _resultFormatter;

    // Temporary only, until the TimeProvider is exposed
    public TelemetryResilienceStrategy(
        string builderName,
        string? strategyKey,
        ILoggerFactory loggerFactory,
        Func<ResilienceContext, object?, object?> resultFormatter,
        List<Action<EnrichmentContext>> enrichers)
        : this(TimeProvider.System, builderName, strategyKey, loggerFactory, resultFormatter, enrichers)
    {
    }

    public TelemetryResilienceStrategy(
        TimeProvider timeProvider,
        string? builderName,
        string? strategyKey,
        ILoggerFactory loggerFactory,
        Func<ResilienceContext, object?, object?> resultFormatter,
        List<Action<EnrichmentContext>> enrichers)
    {
        _timeProvider = timeProvider;
        _builderName = builderName;
        _strategyKey = strategyKey;
        _resultFormatter = resultFormatter;
        _enrichers = enrichers;
        _logger = loggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        ExecutionDuration = ResilienceTelemetryDiagnosticSource.Meter.CreateHistogram<double>(
            "strategy-execution-duration",
            unit: "ms",
            description: "The execution duration and execution result of resilience strategies.");
    }

    public Histogram<double> ExecutionDuration { get; }

    protected override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var stamp = _timeProvider.GetTimestamp();
        Log.ExecutingStrategy(_logger, _builderName, _strategyKey, context.GetResultType());

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        var duration = _timeProvider.GetElapsedTime(stamp);
        var logLevel = context.IsExecutionHealthy() ? LogLevel.Debug : LogLevel.Warning;

        Log.StrategyExecuted(
            _logger,
            logLevel,
            _builderName,
            _strategyKey,
            context.GetResultType(),
            ExpandOutcome(context, outcome),
            context.GetExecutionHealth(),
            duration.TotalMilliseconds,
            outcome.Exception);

        RecordDuration(context, outcome, duration);

        return outcome;
    }

    private static Outcome<object> CreateOutcome<TResult>(Outcome<TResult> outcome) => outcome.HasResult ?
        new Outcome<object>(outcome.Result) :
        new Outcome<object>(outcome.Exception!);

    private void RecordDuration<TResult>(ResilienceContext context, Outcome<TResult> outcome, TimeSpan duration)
    {
        if (!ExecutionDuration.Enabled)
        {
            return;
        }

        var enrichmentContext = EnrichmentContext.Get(context, null, CreateOutcome(outcome));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.BuilderName, _builderName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.StrategyKey, _strategyKey));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ResultType, context.GetResultType()));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExceptionName, outcome.Exception?.GetType().FullName));
        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExecutionHealth, context.GetExecutionHealth()));
        EnrichmentUtil.Enrich(enrichmentContext, _enrichers);

        ExecutionDuration.Record(duration.TotalMilliseconds, enrichmentContext.TagsSpan);
        EnrichmentContext.Return(enrichmentContext);
    }

    private object? ExpandOutcome<TResult>(ResilienceContext context, Outcome<TResult> outcome)
    {
        // stryker disable once all: no means to test this
        return (object)outcome.Exception?.Message! ?? _resultFormatter(context, outcome.Result);
    }
}
