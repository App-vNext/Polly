namespace Polly.Telemetry;

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
    /// Gets a value indicating whether telemetry is enabled.
    /// </summary>
    public bool IsEnabled => DiagnosticSource is not null;

    /// <summary>
    /// Reports an event that occurred in a resilience strategy.
    /// </summary>
    /// <typeparam name="TArgs">The arguments associated with this event.</typeparam>
    /// <param name="resilienceEvent">The reported resilience event.</param>
    /// <param name="context">The resilience context associated with this event.</param>
    /// <param name="args">The event arguments.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public void Report<TArgs>(ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args)
    {
        Guard.NotNull(context);

        context.AddResilienceEvent(resilienceEvent);

        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(resilienceEvent.EventName) || resilienceEvent.Severity == ResilienceEventSeverity.None)
        {
            return;
        }

        var telemetryArgs = TelemetryEventArguments.Get(TelemetrySource, resilienceEvent, context, null, args!);

#pragma warning disable IL2026 // The consumer of this method is Polly.Extensions and it does not use reflection at all
#pragma warning disable IL3050
        DiagnosticSource.Write(resilienceEvent.EventName, telemetryArgs);
#pragma warning restore IL3050
#pragma warning restore IL2026

        TelemetryEventArguments.Return(telemetryArgs);
    }

    /// <summary>
    /// Reports an event that occurred in a resilience strategy.
    /// </summary>
    /// <typeparam name="TArgs">The arguments associated with this event.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="resilienceEvent">The reported resilience event.</param>
    /// <param name="args">The event arguments.</param>
    public void Report<TArgs, TResult>(ResilienceEvent resilienceEvent, OutcomeArguments<TResult, TArgs> args)
    {
        args.Context.AddResilienceEvent(resilienceEvent);

        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(resilienceEvent.EventName) || resilienceEvent.Severity == ResilienceEventSeverity.None)
        {
            return;
        }

        var telemetryArgs = TelemetryEventArguments.Get(TelemetrySource, resilienceEvent, args.Context, args.Outcome.AsOutcome(), args.Arguments!);

#pragma warning disable IL2026 // The consumer of this method is Polly.Extensions and it does not use reflection at all
#pragma warning disable IL3050
        DiagnosticSource.Write(resilienceEvent.EventName, telemetryArgs);
#pragma warning restore IL3050
#pragma warning restore IL2026

        TelemetryEventArguments.Return(telemetryArgs);
    }
}

