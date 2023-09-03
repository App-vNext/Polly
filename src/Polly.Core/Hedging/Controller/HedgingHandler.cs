namespace Polly.Hedging.Utils;

internal sealed record class HedgingHandler<T>(
    Func<HedgingPredicateArguments<T>, ValueTask<bool>> ShouldHandle,
    Func<HedgingActionGeneratorArguments<T>, Func<ValueTask<Outcome<T>>>?> ActionGenerator)
{
    public Func<ValueTask<Outcome<T>>>? GenerateAction(HedgingActionGeneratorArguments<T> args)
    {
        var copiedArgs = new HedgingActionGeneratorArguments<T>(
            args.PrimaryContext,
            args.ActionContext,
            args.AttemptNumber,
            args.Callback);

        return ActionGenerator(copiedArgs);
    }
}

