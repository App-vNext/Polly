using Polly.Telemetry;

namespace Polly.TestUtils;

public sealed class CallbackTelemetryListener : TelemetryListener
{
    private readonly Action<TelemetryEventArguments<object, object>> _callback;
    private readonly object _syncRoot = new();

    public CallbackTelemetryListener(Action<TelemetryEventArguments<object, object>> callback) => _callback = callback;

    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        lock (_syncRoot)
        {
            _callback(args.AsObjectArguments());
        }
    }
}
