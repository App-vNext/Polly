namespace Polly.Utils;

internal static class DefaultPredicates<TArgs, TResult>
    where TArgs : IOutcomeArguments<TResult>
{
    public static readonly Func<TArgs, ValueTask<bool>> HandleOutcome = args => new(args.Outcome.Exception is { } and not OperationCanceledException);
}
