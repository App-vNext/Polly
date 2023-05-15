namespace Polly.Strategy;

/// <summary>
/// Represents a resilience event that has been reported.
/// </summary>
/// <param name="EventName">The event name.</param>
public readonly record struct ReportedResilienceEvent(string EventName)
{
    /// <summary>
    /// Returns an <see cref="EventName"/>.
    /// </summary>
    /// <returns>An event name.</returns>
    public override string ToString() => EventName;
}
