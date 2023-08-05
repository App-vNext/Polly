namespace Polly.Telemetry;

/// <summary>
/// The arguments of the telemetry event.
/// </summary>
public sealed partial class TelemetryEventArguments
{
    private TelemetryEventArguments()
    {
    }

    /// <summary>
    /// Gets the source of the event.
    /// </summary>
    public ResilienceTelemetrySource Source { get; private set; } = null!;

    /// <summary>
    /// Gets the event.
    /// </summary>
    public ResilienceEvent Event { get; private set; }

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
