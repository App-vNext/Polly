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
    /// <param name="eventName">The event name.</param>
    /// <param name="context">The resilience context associated with this event.</param>
    /// <param name="args">The event arguments.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventName"/> is <see langword="null"/>.</exception>
    public void Report<TArgs>(string eventName, ResilienceContext context, TArgs args)
    {
        Guard.NotNull(eventName);
        Guard.NotNull(context);

        AddResilienceEvent(eventName, context, args);

        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(eventName))
        {
            return;
        }

        var telemetryArgs = TelemetryEventArguments.Get(TelemetrySource, eventName, context, null, args!);

        DiagnosticSource.Write(eventName, telemetryArgs);

        TelemetryEventArguments.Return(telemetryArgs);
    }

    /// <summary>
    /// Reports an event that occurred in a resilience strategy.
    /// </summary>
    /// <typeparam name="TArgs">The arguments associated with this event.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">The event arguments.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventName"/> is <see langword="null"/>.</exception>
    public void Report<TArgs, TResult>(string eventName, OutcomeArguments<TResult, TArgs> args)
    {
        Guard.NotNull(eventName);

        AddResilienceEvent(eventName, args.Context, args.Arguments);

        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(eventName))
        {
            return;
        }

        var telemetryArgs = TelemetryEventArguments.Get(TelemetrySource, eventName, args.Context, args.Outcome.AsOutcome(), args.Arguments!);

        DiagnosticSource.Write(eventName, telemetryArgs);

        TelemetryEventArguments.Return(telemetryArgs);
    }

    private static void AddResilienceEvent<TArgs>(string eventName, ResilienceContext context, TArgs args)
    {
        // ExecutionAttemptArguments is not reported as resilience event because that information is already contained
        // in OnHedgingArguments and OnRetryArguments
        if (args is ExecutionAttemptArguments attempt)
        {
            return;
        }

        context.AddResilienceEvent(new ResilienceEvent(eventName));
    }
}

