using Polly.Telemetry;

namespace Polly.Strategy;

/// <summary>
/// Resilience telemetry is used by individual resilience strategies to report some important events.
/// </summary>
/// <remarks>
/// For example, the timeout strategy reports "OnTimeout" event when the timeout is reached or "OnRetry" for retry strategy.
/// </remarks>
public sealed class ResilienceStrategyTelemetry
{
    internal ResilienceStrategyTelemetry(ResilienceTelemetrySource source, DiagnosticSource? diagnosticSource)
    {
        TelemetrySource = source;
        DiagnosticSource = diagnosticSource;
    }

    internal DiagnosticSource? DiagnosticSource { get; }

    internal ResilienceTelemetrySource TelemetrySource { get; }

    /// <summary>
    /// Reports an event that occurred in the resilience strategy.
    /// </summary>
    /// <typeparam name="TArgs">The arguments associated with this event.</typeparam>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">The event arguments.</param>
    public void Report<TArgs>(string eventName, TArgs args)
        where TArgs : IResilienceArguments
    {
        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(eventName))
        {
            return;
        }

        DiagnosticSource.Write(eventName, new TelemetryEventArguments(TelemetrySource, eventName, args, null));
    }

    /// <summary>
    /// Reports an event that occurred in the resilience strategy.
    /// </summary>
    /// <typeparam name="TArgs">The arguments associated with this event.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="eventName">The event name.</param>
    /// <param name="outcome">The outcome associated with the event.</param>
    /// <param name="args">The event arguments.</param>
    public void Report<TArgs, TResult>(string eventName, Outcome<TResult> outcome, TArgs args)
        where TArgs : IResilienceArguments
    {
        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(eventName))
        {
            return;
        }

        DiagnosticSource.Write(eventName, new TelemetryEventArguments(TelemetrySource, eventName, args, outcome.AsOutcome()));
    }
}

