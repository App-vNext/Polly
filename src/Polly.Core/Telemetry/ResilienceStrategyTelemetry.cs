using System.Diagnostics.CodeAnalysis;

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
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "The reflection is not used when consuming the event.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "The reflection is not used when consuming the event.")]
    public void Report<TArgs>(ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args)
    {
        Guard.NotNull(context);

        context.AddResilienceEvent(resilienceEvent);

        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(resilienceEvent.EventName) || resilienceEvent.Severity == ResilienceEventSeverity.None)
        {
            return;
        }

        var telemetryArgs = TelemetryEventArguments.Get(TelemetrySource, resilienceEvent, context, null, args!);

        DiagnosticSource.Write(resilienceEvent.EventName, telemetryArgs);

        TelemetryEventArguments.Return(telemetryArgs);
    }

    /// <summary>
    /// Reports an event that occurred in a resilience strategy.
    /// </summary>
    /// <typeparam name="TArgs">The arguments associated with this event.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="resilienceEvent">The reported resilience event.</param>
    /// <param name="args">The event arguments.</param>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "The reflection is not used when consuming the event.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "The reflection is not used when consuming the event.")]
    public void Report<TArgs, TResult>(ResilienceEvent resilienceEvent, OutcomeArguments<TResult, TArgs> args)
    {
        args.Context.AddResilienceEvent(resilienceEvent);

        if (DiagnosticSource is null || !DiagnosticSource.IsEnabled(resilienceEvent.EventName) || resilienceEvent.Severity == ResilienceEventSeverity.None)
        {
            return;
        }

        var telemetryArgs = TelemetryEventArguments.Get(TelemetrySource, resilienceEvent, args.Context, args.Outcome.AsOutcome(), args.Arguments!);

        DiagnosticSource.Write(resilienceEvent.EventName, telemetryArgs);

        TelemetryEventArguments.Return(telemetryArgs);
    }
}

