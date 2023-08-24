namespace Polly.Hedging.Utils;

internal sealed record class HedgingHandler<T>(
    Func<HedgingPredicateArguments<T>, ValueTask<bool>> ShouldHandle,
    Func<HedgingActionGeneratorArguments<T>, Func<ValueTask<Outcome<T>>>?> ActionGenerator,
    bool IsGeneric)
{
    public Func<ValueTask<Outcome<T>>>? GenerateAction(HedgingActionGeneratorArguments<T> args)
    {
        if (IsGeneric)
        {
            var copiedArgs = new HedgingActionGeneratorArguments<T>(
                args.PrimaryContext,
                args.ActionContext,
                args.AttemptNumber,
                (Func<ResilienceContext, ValueTask<Outcome<T>>>)(object)args.Callback);

            return (Func<ValueTask<Outcome<T>>>?)(object)ActionGenerator(copiedArgs)!;
        }

        return CreateNonGenericAction(args);
    }

    private Func<ValueTask<Outcome<T>>>? CreateNonGenericAction(HedgingActionGeneratorArguments<T> args)
    {
        var generator = (Func<HedgingActionGeneratorArguments<object>, Func<ValueTask<Outcome<object>>>?>)(object)ActionGenerator;
        var action = generator(new HedgingActionGeneratorArguments<object>(args.PrimaryContext, args.ActionContext, args.AttemptNumber, async context =>
        {
            var outcome = await args.Callback(context).ConfigureAwait(context.ContinueOnCapturedContext);
            return outcome.AsOutcome();
        }));

        if (action is null)
        {
            return null;
        }

        return async () =>
        {
            var outcome = await action().ConfigureAwait(args.ActionContext.ContinueOnCapturedContext);
            return outcome.AsOutcome<T>();
        };
    }
}

