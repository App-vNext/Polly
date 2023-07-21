namespace Polly.Telemetry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents a resilience event that has been reported.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct ResilienceEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceEvent"/> struct.
    /// </summary>
    /// <param name="severity">The severity of the event.</param>
    /// <param name="eventName">The event name.</param>
    public ResilienceEvent(ResilienceEventSeverity severity, string eventName)
    {
        Severity = severity;
        EventName = eventName;
    }

    /// <summary>
    /// Gets the severity of the event.
    /// </summary>
    public ResilienceEventSeverity Severity { get; }

    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Returns an <see cref="EventName"/>.
    /// </summary>
    /// <returns>An event name.</returns>
    public override string ToString() => EventName;
}
