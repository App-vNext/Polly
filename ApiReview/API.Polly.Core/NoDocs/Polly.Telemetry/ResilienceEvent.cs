// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public readonly struct ResilienceEvent
{
    public ResilienceEventSeverity Severity { get; }
    public string EventName { get; }
    public ResilienceEvent(ResilienceEventSeverity severity, string eventName);
    public override string ToString();
}
