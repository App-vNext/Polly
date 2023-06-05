using Polly.Strategy;

namespace Polly.Telemetry;

internal sealed record class TelemetryEventArguments(
    ResilienceTelemetrySource Source,
    string EventName,
    ResilienceContext Context,
    Outcome<object>? Outcome,
    object Arguments)
{
}
