using Polly.Telemetry;

namespace Polly.TestUtils;

public sealed class FakeTelemetryListener : TelemetryListener
{
    private readonly Action<TelemetryEventArguments<object, object>> _callback;
    private readonly object _syncRoot = new();

    public FakeTelemetryListener(Action<TelemetryEventArguments<object, object>> callback) => _callback = callback;

    public FakeTelemetryListener() => _callback = _ => { };

    public IList<TelemetryEventArguments<object, object>> Events { get; } = [];

    public IEnumerable<T> GetArgs<T>() => Events.Select(e => e.Arguments).OfType<T>();

    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        lock (_syncRoot)
        {
            Events.Add(args.AsObjectArguments());
            _callback(args.AsObjectArguments());
        }
    }
}
