// Assembly 'Polly.Core'

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public sealed class ResilienceStrategyRegistry<TKey> : ResilienceStrategyProvider<TKey> where TKey : notnull
{
    public ResilienceStrategyRegistry();
    public ResilienceStrategyRegistry(ResilienceStrategyRegistryOptions<TKey> options);
    public bool TryAdd(TKey key, ResilienceStrategy strategy);
    public bool TryAdd<TResult>(TKey key, ResilienceStrategy<TResult> strategy);
    public bool Remove(TKey key);
    public bool Remove<TResult>(TKey key);
    public override bool TryGet<TResult>(TKey key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy);
    public override bool TryGet(TKey key, [NotNullWhen(true)] out ResilienceStrategy? strategy);
    public bool TryAddBuilder(TKey key, Action<ResilienceStrategyBuilder, ConfigureBuilderContext<TKey>> configure);
    public bool TryAddBuilder<TResult>(TKey key, Action<ResilienceStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>> configure);
    public bool RemoveBuilder(TKey key);
    public bool RemoveBuilder<TResult>(TKey key);
    public void Clear();
    public void Clear<TResult>();
}
