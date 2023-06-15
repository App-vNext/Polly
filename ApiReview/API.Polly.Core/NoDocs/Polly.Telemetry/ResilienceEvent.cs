// Assembly 'Polly.Core'

namespace Polly.Telemetry;

public readonly record struct ResilienceEvent(string EventName)
{
    public override string ToString();
}
