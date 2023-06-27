namespace Polly.Telemetry;

/// <summary>
/// Represents a resilience event that has been reported.
/// </summary>
/// <param name="Severity">The severity of the event.</param>
/// <param name="EventName">The event name.</param>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly record struct ResilienceEvent(ResilienceEventSeverity Severity, string EventName)
{
    /// <summary>
    /// Returns an <see cref="EventName"/>.
    /// </summary>
    /// <returns>An event name.</returns>
    public override string ToString() => EventName;
}
