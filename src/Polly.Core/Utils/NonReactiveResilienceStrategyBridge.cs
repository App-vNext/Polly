namespace Polly.Utils;

[DebuggerDisplay("{Strategy}")]
internal sealed class NonReactiveResilienceStrategyBridge : ResilienceStrategy
{
    public NonReactiveResilienceStrategyBridge(NonReactiveResilienceStrategy strategy) => Strategy = strategy;

    public NonReactiveResilienceStrategy Strategy { get; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Strategy.ExecuteCore(callback, context, state);
}
