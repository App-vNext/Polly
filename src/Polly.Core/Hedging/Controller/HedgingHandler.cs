namespace Polly.Hedging.Utils;

internal sealed record class HedgingHandler<T>(
    PredicateInvoker<HedgingPredicateArguments> ShouldHandle,
    Func<HedgingActionGeneratorArguments<T>, Func<ValueTask<Outcome<T>>>?> ActionGenerator,
    bool IsGeneric)
{
    public bool HandlesHedging<TResult>() => IsGeneric switch
    {
        true => typeof(TResult) == typeof(T),
        false => true
    };

    public Func<ValueTask<Outcome<TResult>>>? GenerateAction<TResult>(HedgingActionGeneratorArguments<TResult> args)
    {
        if (IsGeneric)
        {
            var copiedArgs = new HedgingActionGeneratorArguments<T>(
                args.PrimaryContext,
                args.ActionContext,
                args.Attempt,
                (Func<ResilienceContext, ValueTask<Outcome<T>>>)(object)args.Callback);

            return (Func<ValueTask<Outcome<TResult>>>?)(object)ActionGenerator(copiedArgs)!;
        }

        return CreateNonGenericAction(args);
    }

    private Func<ValueTask<Outcome<TResult>>>? CreateNonGenericAction<TResult>(HedgingActionGeneratorArguments<TResult> args)
    {
        var generator = (Func<HedgingActionGeneratorArguments<object>, Func<ValueTask<Outcome<object>>>?>)(object)ActionGenerator;
        var action = generator(new HedgingActionGeneratorArguments<object>(args.PrimaryContext, args.ActionContext, args.Attempt, async context =>
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
            return outcome.AsOutcome<TResult>();
        };
    }
}

