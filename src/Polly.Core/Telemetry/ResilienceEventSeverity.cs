namespace Polly.Telemetry;

/// <summary>
/// The severity of reported resilience event.
/// </summary>
public enum ResilienceEventSeverity
{
    /// <summary>
    /// The resilience event is informational.
    /// </summary>
    Information,

    /// <summary>
    /// The resilience event should be treated as a warning.
    /// </summary>
    Warning,

    /// <summary>
    /// The resilience event should be treated as a warning.
    /// </summary>
    Error,
}
