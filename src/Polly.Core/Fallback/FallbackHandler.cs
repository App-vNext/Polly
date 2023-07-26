namespace Polly.Fallback;

internal sealed record class FallbackHandler<T>(
    Func<OutcomeArguments<T, FallbackPredicateArguments>, ValueTask<bool>> ShouldHandle,
    Func<OutcomeArguments<T, FallbackPredicateArguments>, ValueTask<Outcome<T>>> ActionGenerator)
{
    public async ValueTask<Outcome<TResult>> GetFallbackOutcomeAsync<TResult>(OutcomeArguments<TResult, FallbackPredicateArguments> args)
    {
        var copiedArgs = new OutcomeArguments<T, FallbackPredicateArguments>(
            args.Context,
            args.Outcome.AsOutcome<T>(),
            args.Arguments);

        return (await ActionGenerator(copiedArgs).ConfigureAwait(args.Context.ContinueOnCapturedContext)).AsOutcome<TResult>();
    }
}

