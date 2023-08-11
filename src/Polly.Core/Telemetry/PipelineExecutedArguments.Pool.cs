namespace Polly.Telemetry;

public sealed partial class PipelineExecutedArguments
{
    private static readonly ObjectPool<PipelineExecutedArguments> Pool = new(() => new PipelineExecutedArguments(), args =>
    {
        args.Duration = TimeSpan.Zero;
    });

    internal static PipelineExecutedArguments Get(TimeSpan duration)
    {
        var args = Pool.Get();
        args.Duration = duration;
        return args;
    }

    internal static void Return(PipelineExecutedArguments args) => Pool.Return(args);
}
