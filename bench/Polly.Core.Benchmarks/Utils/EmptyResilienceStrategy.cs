namespace Polly.Core.Benchmarks.Utils;

internal class EmptyResilienceStrategy : NonReactiveResilienceStrategy
{
    protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return callback(context, state);
    }
}
