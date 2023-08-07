using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Polly.Extensions.Telemetry;

internal sealed class TelemetryResilienceStrategy<T> : ResilienceStrategy<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly string? _builderName;
    private readonly string? _builderInstance;
    private readonly List<Action<EnrichmentContext>> _enrichers;
    private readonly ILogger _logger;
    private readonly Func<ResilienceContext, object?, object?> _resultFormatter;

    // Temporary only, until the TimeProvider is exposed
    public TelemetryResilienceStrategy(
        string builderName,
        string? builderInstance,
        ILoggerFactory loggerFactory,
        Func<ResilienceContext, object?, object?> resultFormatter,
        List<Action<EnrichmentContext>> enrichers)
        : this(TimeProvider.System, builderName, builderInstance, loggerFactory, resultFormatter, enrichers)
    {
    }

    public TelemetryResilienceStrategy(
        TimeProvider timeProvider,
        string? builderName,
        string? builderInstance,
        ILoggerFactory loggerFactory,
        Func<ResilienceContext, object?, object?> resultFormatter,
        List<Action<EnrichmentContext>> enrichers)
    {
        _timeProvider = timeProvider;
        _builderName = builderName;
        _builderInstance = builderInstance;
        _resultFormatter = resultFormatter;
        _enrichers = enrichers;
        _logger = loggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        ExecutionDuration = ResilienceTelemetryDiagnosticSource.Meter.CreateHistogram<double>(
            "strategy-execution-duration",
            unit: "ms",
            description: "The execution duration and execution results of resilience strategies.");
    }

    public Histogram<double> ExecutionDuration { get; }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var stamp = _timeProvider.GetTimestamp();
        Log.ExecutingStrategy(_logger, _builderName.GetValueOrPlaceholder(), _builderInstance.GetValueOrPlaceholder(), context.OperationKey, context.GetResultType());

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        var duration = _timeProvider.GetElapsedTime(stamp);
        var logLevel = context.IsExecutionHealthy() ? LogLevel.Debug : LogLevel.Warning;

        Log.StrategyExecuted(
            _logger,
            logLevel,
            _builderName.GetValueOrPlaceholder(),
            _builderInstance.GetValueOrPlaceholder(),
            context.OperationKey,
            context.GetResultType(),
            ExpandOutcome(context, outcome),
            context.GetExecutionHealth(),
            duration.TotalMilliseconds,
            outcome.Exception);

        RecordDuration(context, outcome, duration);

        return outcome;
    }

    private static Outcome<object> CreateOutcome<TResult>(Outcome<TResult> outcome) => outcome.HasResult ?
        Outcome.FromResult<object>(outcome.Result) :
        Outcome.FromException<object>(outcome.Exception!);

    private void RecordDuration<TResult>(ResilienceContext context, Outcome<TResult> outcome, TimeSpan duration)
    {
        if (!ExecutionDuration.Enabled)
        {
            return;
        }

        var enrichmentContext = EnrichmentContext.Get(context, null, CreateOutcome(outcome));

        if (_builderName is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.BuilderName, _builderName));
        }

        if (_builderInstance is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.BuilderInstance, _builderInstance));
        }

        if (context.OperationKey is not null)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.OperationKey, context.OperationKey));
        }

        enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ResultType, context.GetResultType()));

        if (outcome.Exception is Exception e)
        {
            enrichmentContext.Tags.Add(new(ResilienceTelemetryTags.ExceptionName, e.GetType().FullName));
        }

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
