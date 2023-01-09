namespace Polly.Internals;

internal sealed class DelegatingStrategyWrapper : DelegatingResilienceStrategy
{
    private readonly IResilienceStrategy _strategy;

    public DelegatingStrategyWrapper(IResilienceStrategy strategy)
    {
        _strategy = strategy;
    }

    public override ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state)
    {
        return _strategy.ExecuteAsync(
            static (context, state) => state.Next.ExecuteAsync(state.execution, context, state.state),
            context,
            (Next, execution, state));
    }
}
