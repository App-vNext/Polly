namespace Polly.Fallback;

internal sealed record class FallbackHandler<T>(
    PredicateInvoker<HandleFallbackArguments> ShouldHandle,
    Func<OutcomeArguments<T, HandleFallbackArguments>, ValueTask<Outcome<T>>> ActionGenerator,
    bool IsGeneric)
{
    public bool HandlesFallback<TResult>() => IsGeneric switch
    {
        true => typeof(TResult) == typeof(T),
        _ => true
    };

    public async ValueTask<Outcome<TResult>> GetFallbackOutcomeAsync<TResult>(OutcomeArguments<TResult, HandleFallbackArguments> args)
    {
        var copiedArgs = new OutcomeArguments<T, HandleFallbackArguments>(
            args.Context,
            args.Outcome.AsOutcome<T>(),
            args.Arguments);

        return (await ActionGenerator(copiedArgs).ConfigureAwait(args.Context.ContinueOnCapturedContext)).AsOutcome<TResult>();
    }
}

