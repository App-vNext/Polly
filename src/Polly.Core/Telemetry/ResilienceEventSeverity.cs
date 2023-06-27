namespace Polly.Telemetry;

/// <summary>
/// The severity of reported resilience event.
/// </summary>
public enum ResilienceEventSeverity
{
    /// <summary>
    /// The resilience event is not recorded.
    /// </summary>
    None = 0,

    /// <summary>
    /// The resilience event is used for debugging purposes only.
    /// </summary>
    Debug,

    /// <summary>
    /// The resilience event is informational.
    /// </summary>
    Information,

    /// <summary>
    /// The resilience event should be treated as a warning.
    /// </summary>
    Warning,

    /// <summary>
    /// The resilience event should be treated as an error.
    /// </summary>
    Error,

    /// <summary>
    /// The resilience event should be treated as a critical error.
    /// </summary>
    Critical,
}
