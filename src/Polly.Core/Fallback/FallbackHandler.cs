namespace Polly.Fallback;

internal sealed record class FallbackHandler<T>(
    Func<FallbackPredicateArguments<T>, ValueTask<bool>> ShouldHandle,
    Func<FallbackActionArguments<T>, ValueTask<Outcome<T>>> ActionGenerator)
{
    public async ValueTask<Outcome<TResult>> GetFallbackOutcomeAsync<TResult>(FallbackActionArguments<T> args)
    {
        var copiedArgs = new FallbackActionArguments<T>(
            args.Context,
            args.Outcome.AsOutcome<T>());

        return (await ActionGenerator(copiedArgs).ConfigureAwait(args.Context.ContinueOnCapturedContext)).AsOutcome<TResult>();
    }
}

