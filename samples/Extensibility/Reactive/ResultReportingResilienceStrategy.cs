using Polly;
using Polly.Telemetry;

namespace Extensibility.Reactive;

#region ext-reactive-strategy

// Strategies should be internal and not exposed in the library's public API.
// Use extension methods and options to configure the strategy.
internal sealed class ResultReportingResilienceStrategy<T> : ResilienceStrategy<T>
{
    private readonly Func<ResultReportingPredicateArguments<T>, ValueTask<bool>> _shouldHandle;
    private readonly Func<OnReportResultArguments<T>, ValueTask> _onReportResult;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public ResultReportingResilienceStrategy(
        Func<ResultReportingPredicateArguments<T>, ValueTask<bool>> shouldHandle,
        Func<OnReportResultArguments<T>, ValueTask> onReportResult,
        ResilienceStrategyTelemetry telemetry)
    {
        _shouldHandle = shouldHandle;
        _onReportResult = onReportResult;
        _telemetry = telemetry;
    }

    protected override async ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Execute the given callback and adhere to the ContinueOnCapturedContext property value.
        Outcome<T> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        // Check if the outcome should be reported using the "ShouldHandle" predicate.
        if (await _shouldHandle(new ResultReportingPredicateArguments<T>(context, outcome)).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            // Bundle information about the event into arguments.
            var args = new OnReportResultArguments<T>(context, outcome);

            // Report this as a resilience event with information severity level to the telemetry infrastructure.
            _telemetry.Report(
                new ResilienceEvent(ResilienceEventSeverity.Information, "ResultReported"),
                context,
                outcome,
                args);

            // Call the "OnReportResult" callback.
            await _onReportResult(args).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }
}

#endregion
