namespace Polly.Telemetry;

public sealed partial class TelemetryEventArguments
{
    private static readonly ObjectPool<TelemetryEventArguments> Pool = new(() => new TelemetryEventArguments(), args =>
    {
        args.Source = null!;
        args.Event = default;
        args.Context = null!;
        args.Outcome = default;
        args.Arguments = null!;
    });

    internal static TelemetryEventArguments Get(
        ResilienceTelemetrySource source,
        ResilienceEvent resilienceEvent,
        ResilienceContext context,
        Outcome<object>? outcome,
        object arguments)
    {
        var args = Pool.Get();

        args.Source = source;
        args.Event = resilienceEvent;
        args.Context = context;
        args.Outcome = outcome;
        args.Arguments = arguments;

        return args;
    }

    internal static void Return(TelemetryEventArguments args) => Pool.Return(args);
}
