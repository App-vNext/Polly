namespace Polly.Telemetry;

/// <summary>
/// Represents a resilience event that has been reported.
/// </summary>
/// <param name="EventName">The event name.</param>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly record struct ResilienceEvent(string EventName)
{
    /// <summary>
    /// Returns an <see cref="EventName"/>.
    /// </summary>
    /// <returns>An event name.</returns>
    public override string ToString() => EventName;
}
