// Assembly 'Polly.Core'

namespace Polly.Telemetry;

public readonly record struct ResilienceEvent(ResilienceEventSeverity Severity, string EventName)
{
    public override string ToString();
}
