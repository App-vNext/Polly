using System.ComponentModel;
using Polly.Strategy;

namespace Polly.Telemetry;

/// <summary>
/// The arguments of the telemetry event.
/// </summary>
/// <param name="Source">The source of the event.</param>
/// <param name="EventName">The event name.</param>
/// <param name="Context">The resilience context.</param>
/// <param name="Outcome">The outcome of an execution.</param>
/// <param name="Arguments">The arguments associated with the event.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record class TelemetryEventArguments(
    ResilienceTelemetrySource Source,
    string EventName,
    ResilienceContext Context,
    Outcome<object>? Outcome,
    object Arguments)
{
}
