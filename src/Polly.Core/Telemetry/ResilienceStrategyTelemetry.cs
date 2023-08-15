namespace Polly.Telemetry;

/// <summary>
/// Resilience telemetry is used by individual resilience strategies to report some important events.
/// </summary>
/// <remarks>
/// For example, the timeout strategy reports "OnTimeout" event when the timeout is reached or "OnRetry" for retry strategy.
/// </remarks>
public sealed class ResilienceStrategyTelemetry
{
    internal ResilienceStrategyTelemetry(ResilienceTelemetrySource source, TelemetryListener? listener)
    {
        TelemetrySource = source;
        Listener = listener;
    }

    internal TelemetryListener? Listener { get; }

    internal ResilienceTelemetrySource TelemetrySource { get; }

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

        if (Listener is null || resilienceEvent.Severity == ResilienceEventSeverity.None)
        {
            return;
        }

        Listener.Write<object, TArgs>(new(TelemetrySource, resilienceEvent, context, args, null));
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

        if (Listener is null || resilienceEvent.Severity == ResilienceEventSeverity.None)
        {
            return;
        }

        Listener.Write<TResult, TArgs>(new(TelemetrySource, resilienceEvent, args.Context, args.Arguments, args.Outcome));
    }
}

