namespace Polly.Telemetry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="TelemetryOptions.SeverityProvider"/>.
/// </summary>
public readonly struct SeverityProviderArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeverityProviderArguments"/> struct.
    /// </summary>
    /// <param name="source">The source that produced the resilience event.</param>
    /// <param name="resilienceEvent">The resilience event.</param>
    /// <param name="context">The resilience context.</param>
    public SeverityProviderArguments(ResilienceTelemetrySource source, ResilienceEvent resilienceEvent, ResilienceContext context)
    {
        Source = source;
        Event = resilienceEvent;
        Context = context;
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
    /// Gets the resilience context.
    /// </summary>
    public ResilienceContext Context { get; }
}
