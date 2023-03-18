using Polly;

namespace Polly.Core.Benchmarks;

internal class EmptyResilienceStrategy : ResilienceStrategy
{
    protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return callback(context, state);
    }
}
