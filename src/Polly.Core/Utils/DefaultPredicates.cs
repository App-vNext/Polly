namespace Polly.Utils;

internal static class DefaultPredicates<TArgs, TResult>
    where TArgs : IOutcomeArguments<TResult>
{
    public static readonly Func<TArgs, ValueTask<bool>> HandleOutcome = args => args.Outcome.Exception switch
    {
        OperationCanceledException => PredicateResult.False(),
        Exception => PredicateResult.True(),
        _ => PredicateResult.False(),
    };
}
