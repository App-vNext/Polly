namespace Polly.Fallback;

internal sealed record class FallbackHandler<T>(
    Func<FallbackPredicateArguments<T>, ValueTask<bool>> ShouldHandle,
    Func<FallbackPredicateArguments<T>, ValueTask<Outcome<T>>> ActionGenerator)
{
    public async ValueTask<Outcome<TResult>> GetFallbackOutcomeAsync<TResult>(FallbackPredicateArguments<T> args)
    {
        var copiedArgs = new FallbackPredicateArguments<T>(
            args.Context,
            args.Outcome.AsOutcome<T>());

        return (await ActionGenerator(copiedArgs).ConfigureAwait(args.Context.ContinueOnCapturedContext)).AsOutcome<TResult>();
    }
}

