namespace Polly.Telemetry;

public sealed partial record class TelemetryEventArguments
{
    private static readonly ObjectPool<TelemetryEventArguments> Pool = new(() => new TelemetryEventArguments(), args =>
    {
        args.Source = null!;
        args.EventName = null!;
        args.Context = null!;
        args.Outcome = default;
        args.Arguments = null!;
    });

    internal static TelemetryEventArguments Get(
        ResilienceTelemetrySource source,
        string eventName,
        ResilienceContext context,
        Outcome<object>? outcome,
        object arguments)
    {
        var args = Pool.Get();

        args.Source = source;
        args.EventName = eventName;
        args.Context = context;
        args.Outcome = outcome;
        args.Arguments = arguments;

        return args;
    }

    internal static void Return(TelemetryEventArguments args) => Pool.Return(args);
}
