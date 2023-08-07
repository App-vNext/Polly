using System;
using System.Threading.Tasks;

namespace Polly.Utils;

#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.

internal sealed class BridgeStrategy<T> : ResilienceStrategy<T>
{
    private readonly ResilienceStrategy<object> _strategy;

    public BridgeStrategy(ResilienceStrategy<object> strategy) => _strategy = strategy;

    protected override ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state) => _strategy.ExecuteCoreAsync(callback, context, state);
}
