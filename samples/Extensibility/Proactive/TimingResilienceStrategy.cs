using Polly;
using Polly.Telemetry;
using System.Diagnostics;

namespace Extensibility.Proactive;

#region ext-proactive-strategy

// The strategies should be internal and not exposed as part of the library's public API.
// The configuration of strategy should be done via extension methods and options.
internal sealed class TimingResilienceStrategy : ResilienceStrategy
{
    private readonly TimeSpan _threshold;

    private readonly Func<ThresholdExceededArguments, ValueTask>? _thresholdExceeded;

    private readonly ResilienceStrategyTelemetry _telemetry;

    public TimingResilienceStrategy(
        TimeSpan threshold,
        Func<ThresholdExceededArguments, ValueTask>? thresholdExceeded,
        ResilienceStrategyTelemetry telemetry)
    {
        _threshold = threshold;
        _telemetry = telemetry;
        _thresholdExceeded = thresholdExceeded;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var stopwatch = Stopwatch.StartNew();

        // Execute the provided callback and respect the value of ContinueOnCapturedContext property.
        Outcome<TResult> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        if (stopwatch.Elapsed > _threshold)
        {
            // Create arguments that encapsulate the information about the event.
            var args = new ThresholdExceededArguments(context, _threshold, stopwatch.Elapsed);

            // Since we detected that this execution took longer than the threshold, we will report this as an resilience event.
            _telemetry.Report(
                new ResilienceEvent(ResilienceEventSeverity.Warning, "ExecutionThresholdExceeded"), // Pass the event severity and the event name
                context, // Forward the context
                 args); // Forward the arguments so any listeners can recognize this particular event

            if (_thresholdExceeded is not null)
            {
                await _thresholdExceeded(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }

        // Just return the outcome
        return outcome;
    }
}

#endregion
