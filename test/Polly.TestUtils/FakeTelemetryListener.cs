using Polly.Telemetry;

#if !NET10_0_OR_GREATER
#pragma warning disable SA1121
using Lock = System.Object;
#endif

namespace Polly.TestUtils;

public sealed class FakeTelemetryListener : TelemetryListener
{
    private readonly Action<TelemetryEventArguments<object, object>> _callback;
    private readonly Lock _syncRoot = new();

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
