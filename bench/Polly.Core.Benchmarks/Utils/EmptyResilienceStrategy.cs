namespace Polly.Core.Benchmarks.Utils;

internal class EmptyResilienceStrategy<T> : ResilienceStrategy<T>
{
    protected override ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state)
    {
        return callback(context, state);
    }
}
