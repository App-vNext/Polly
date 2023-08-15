namespace Polly.Telemetry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the information about the resilience event.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <typeparam name="TArgs">The arguments associated with the resilience event.</typeparam>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly struct TelemetryEventArguments<TResult, TArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryEventArguments{TResult, TArgs}"/> struct.
    /// </summary>
    /// <param name="source">The source that produced the resilience event.</param>
    /// <param name="resilienceEvent">The resilience event.</param>
    /// <param name="context">The context associated with the resilience event.</param>
    /// <param name="args">The arguments associated with the resilience event.</param>
    /// <param name="outcome">The outcome associated with the resilience event, if any.</param>
    public TelemetryEventArguments(ResilienceTelemetrySource source, ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args, Outcome<TResult>? outcome)
    {
        Source = source;
        Event = resilienceEvent;
        Context = context;
        Arguments = args;
        Outcome = outcome;
    }

    /// <summary>
    /// Gets the source that produced the resilience event.
    /// </summary>
    public ResilienceTelemetrySource Source { get; }

    /// <summary>
    /// Gets the resilience event.
    /// </summary>
    public ResilienceEvent Event { get; }

    /// <summary>
    /// Gets the context associated with the resilience event.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the arguments associated with the resilience event.
    /// </summary>
    public TArgs Arguments { get; }

    /// <summary>
    /// Gets the outcome associated with the resilience event, if any.
    /// </summary>
    public Outcome<TResult>? Outcome { get; }
}
