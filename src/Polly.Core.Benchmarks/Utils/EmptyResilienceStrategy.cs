using Polly.Strategy;

namespace Polly.Core.Benchmarks.Utils;

internal class EmptyResilienceStrategy : ResilienceStrategy
{
    protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return callback(context, state);
    }
}
