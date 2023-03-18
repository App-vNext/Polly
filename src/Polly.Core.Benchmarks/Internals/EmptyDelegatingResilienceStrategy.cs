namespace Polly.Core.Benchmarks;

internal class EmptyDelegatingResilienceStrategy : DelegatingResilienceStrategy
{
    protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return base.ExecuteCoreAsync(callback, context, state);
    }
}