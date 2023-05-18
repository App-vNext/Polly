using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

internal class HedgingActions
{
    private readonly TimeProvider _timeProvider;

    public HedgingActions(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        Functions = new()
        {
            GetApples,
            GetOranges,
            GetPears
        };

        Generator = args =>
        {
            if (args.Attempt <= Functions.Count)
            {
                return async () =>
                {
                    return await Functions[args.Attempt - 1]!(args.Context);
                };
            }

            return null;
        };
    }

    public Func<HedgingActionGeneratorArguments<string>, Func<Task<string>>?> Generator { get; }

    public Func<HedgingActionGeneratorArguments<string>, Func<Task<string>>?> EmptyFunctionsProvider { get; } = args => null;

    public List<Func<ResilienceContext, Task<string>>> Functions { get; }

    private async Task<string> GetApples(ResilienceContext context)
    {
        await _timeProvider.DelayAsync(TimeSpan.FromSeconds(10), context.CancellationToken);
        return "Apples";
    }

    private async Task<string> GetPears(ResilienceContext context)
    {
        await _timeProvider.DelayAsync(TimeSpan.FromSeconds(3), context.CancellationToken);
        return "Pears";
    }

    private async Task<string> GetOranges(ResilienceContext context)
    {
        await _timeProvider.DelayAsync(TimeSpan.FromSeconds(2), context.CancellationToken);
        return "Oranges";
    }

    public static Func<HedgingActionGeneratorArguments<string>, Func<Task<string>>?> GetGenerator(Func<ResilienceContext, Task<string>> task) => args => () => task(args.Context);

    public int MaxHedgedTasks => Functions.Count + 1;
}
