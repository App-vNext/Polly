using System.ComponentModel;

namespace Polly.Telemetry;

/// <summary>
/// The arguments of the telemetry event.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial record class TelemetryEventArguments
{
    private TelemetryEventArguments()
    {
    }

    /// <summary>
    /// Gets the source of the event.
    /// </summary>
    public ResilienceTelemetrySource Source { get; private set; } = null!;

    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string EventName { get; private set; } = null!;

    /// <summary>
    /// Gets the resilience context.
    /// </summary>
    public ResilienceContext Context { get; private set; } = null!;

    /// <summary>
    /// Gets the outcome of an execution.
    /// </summary>
    public Outcome<object>? Outcome { get; private set; }

    /// <summary>
    /// Gets the arguments associated with the event.
    /// </summary>
    public object Arguments { get; private set; } = null!;
}
