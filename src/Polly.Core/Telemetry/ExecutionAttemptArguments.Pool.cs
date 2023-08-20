namespace Polly.Telemetry;

public partial class ExecutionAttemptArguments
{
    private static readonly ObjectPool<ExecutionAttemptArguments> Pool = new(() => new ExecutionAttemptArguments(), args =>
    {
        args.Duration = TimeSpan.Zero;
        args.AttemptNumber = 0;
        args.Handled = false;
    });

    internal static ExecutionAttemptArguments Get(int attempt, TimeSpan duration, bool handled)
    {
        var args = Pool.Get();
        args.AttemptNumber = attempt;
        args.Duration = duration;
        args.Handled = handled;
        return args;
    }

    internal static void Return(ExecutionAttemptArguments args) => Pool.Return(args);
}
