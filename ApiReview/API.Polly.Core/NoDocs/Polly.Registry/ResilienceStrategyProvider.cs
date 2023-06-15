// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public abstract class ResilienceStrategyProvider<TKey> where TKey : notnull
{
    public virtual ResilienceStrategy Get(TKey key);
    public virtual ResilienceStrategy<TResult> Get<TResult>(TKey key);
    public abstract bool TryGet(TKey key, [NotNullWhen(true)] out ResilienceStrategy? strategy);
    public abstract bool TryGet<TResult>(TKey key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy);
    protected ResilienceStrategyProvider();
}
