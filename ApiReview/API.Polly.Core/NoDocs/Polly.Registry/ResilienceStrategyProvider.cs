// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public abstract class ResilienceStrategyProvider<TKey> where TKey : notnull
{
    public virtual ResilienceStrategy GetStrategy(TKey key);
    public virtual ResilienceStrategy<TResult> GetStrategy<TResult>(TKey key);
    public abstract bool TryGetStrategy(TKey key, [NotNullWhen(true)] out ResilienceStrategy? strategy);
    public abstract bool TryGetStrategy<TResult>(TKey key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy);
    protected ResilienceStrategyProvider();
}
