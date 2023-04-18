namespace Polly.Strategy;

/// <summary>
/// Represents a resilience event that has been reported.
/// </summary>
/// <param name="EventName">The event name.</param>
public readonly record struct ReportedResilienceEvent(string EventName)
{
    /// <inheritdoc/>
    public override string ToString() => EventName;
}
