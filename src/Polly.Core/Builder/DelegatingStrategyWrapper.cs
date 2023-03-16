using System;

namespace Polly.Builder;

/// <summary>
/// A wrapper that converts a <see cref="IResilienceStrategy"/> into a <see cref="DelegatingResilienceStrategy"/>.
/// </summary>
internal sealed class DelegatingStrategyWrapper : DelegatingResilienceStrategy
{
    private readonly IResilienceStrategy _strategy;

    public DelegatingStrategyWrapper(IResilienceStrategy strategy) => _strategy = strategy;

    protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return _strategy.ExecuteAsync(
            static (context, state) => state.Next.ExecuteAsync(state.callback, context, state.state),
            context,
            (Next, callback, state));
    }
}
