namespace Polly.Fallback;

internal sealed record class FallbackHandler<T>(
    Func<FallbackPredicateArguments<T>, ValueTask<bool>> ShouldHandle,
    Func<FallbackActionArguments<T>, ValueTask<Outcome<T>>> ActionGenerator)
{
    public ValueTask<Outcome<T>> GetFallbackOutcomeAsync(FallbackActionArguments<T> args) => ActionGenerator(args);
}

