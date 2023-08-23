// Assembly 'Polly.Core'

namespace Polly.Telemetry;

public abstract class TelemetryListener
{
    public abstract void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args);
    protected TelemetryListener();
}
