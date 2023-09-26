﻿using Polly;
using Polly.Telemetry;
using System.Diagnostics;

namespace Extensibility.Proactive;

#region ext-proactive-strategy

// Strategies should be internal and not exposed in the library's public API.
// Configure the strategy through extension methods and options.
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

        // Execute the given callback and adhere to the ContinueOnCapturedContext property value.
        Outcome<TResult> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        if (stopwatch.Elapsed > _threshold)
        {
            // Bundle information about the event into arguments.
            var args = new ThresholdExceededArguments(context, _threshold, stopwatch.Elapsed);

            // Report this as a resilience event if the execution took longer than the threshold.
            _telemetry.Report(
                new ResilienceEvent(ResilienceEventSeverity.Warning, "ExecutionThresholdExceeded"),
                context,
                args);

            if (_thresholdExceeded is not null)
            {
                await _thresholdExceeded(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }

        // Return the outcome directly.
        return outcome;
    }
}

#endregion
