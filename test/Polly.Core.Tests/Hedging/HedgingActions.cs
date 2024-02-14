using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

internal class HedgingActions
{
    private readonly TimeProvider _timeProvider;

    public HedgingActions(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        Functions =
        [
            GetApples,
            GetOranges,
            GetPears
        ];

        Generator = args =>
        {
            if (args.AttemptNumber <= Functions.Count)
            {
                return async () =>
                {
                    return await Functions[args.AttemptNumber - 1]!(args.ActionContext);
                };
            }

            return null;
        };
    }

    public Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> Generator { get; }

    public Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> EmptyFunctionsProvider { get; } = args => null;

    public List<Func<ResilienceContext, ValueTask<Outcome<string>>>> Functions { get; }

    private async ValueTask<Outcome<string>> GetApples(ResilienceContext context)
    {
        await _timeProvider.Delay(TimeSpan.FromSeconds(10), context.CancellationToken);
        return Outcome.FromResult("Apples");
    }

    private async ValueTask<Outcome<string>> GetPears(ResilienceContext context)
    {
        await _timeProvider.Delay(TimeSpan.FromSeconds(3), context.CancellationToken);
        return Outcome.FromResult("Pears");
    }

    private async ValueTask<Outcome<string>> GetOranges(ResilienceContext context)
    {
        await _timeProvider.Delay(TimeSpan.FromSeconds(2), context.CancellationToken);
        return Outcome.FromResult("Oranges");
    }

    public static Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> GetGenerator(Func<ResilienceContext, ValueTask<Outcome<string>>> task) =>
        args => () => task(args.ActionContext);

    public int MaxHedgedTasks => Functions.Count + 1;
}
