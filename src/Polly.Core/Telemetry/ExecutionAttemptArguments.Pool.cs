namespace Polly.Telemetry;

public partial class ExecutionAttemptArguments
{
    private static readonly ObjectPool<ExecutionAttemptArguments> Pool = new(() => new ExecutionAttemptArguments(), args =>
    {
        args.ExecutionTime = TimeSpan.Zero;
        args.Attempt = 0;
        args.Handled = false;
    });

    internal static ExecutionAttemptArguments Get(int attempt, TimeSpan executionTime, bool handled)
    {
        var args = Pool.Get();
        args.Attempt = attempt;
        args.ExecutionTime = executionTime;
        args.Handled = handled;
        return args;
    }

    internal static void Return(ExecutionAttemptArguments args) => Pool.Return(args);
}
