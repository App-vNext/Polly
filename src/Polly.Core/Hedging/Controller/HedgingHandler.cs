namespace Polly.Hedging.Utils;

internal sealed record class HedgingHandler<T>(
    Func<HedgingPredicateArguments<T>, ValueTask<bool>> ShouldHandle,
    Func<HedgingActionGeneratorArguments<T>, Func<ValueTask<Outcome<T>>>?> ActionGenerator,
    Func<OnHedgingArguments<T>, ValueTask>? OnHedging)
{
    public readonly bool IsDefaultActionGenerator = ActionGenerator == HedgingStrategyOptions<T>.DefaultActionGenerator;
}
