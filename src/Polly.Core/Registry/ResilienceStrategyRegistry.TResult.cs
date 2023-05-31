using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public sealed partial class ResilienceStrategyRegistry<TKey> : ResilienceStrategyProvider<TKey>
    where TKey : notnull
{
    private sealed class GenericRegistry<TResult>
    {
        private readonly Func<ResilienceStrategyBuilder<TResult>> _activator;
        private readonly ConcurrentDictionary<TKey, Action<ResilienceStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>>> _builders;
        private readonly ConcurrentDictionary<TKey, ResilienceStrategy<TResult>> _strategies;

        private readonly Func<TKey, string> _strategyKeyFormatter;
        private readonly Func<TKey, string> _builderNameFormatter;

        public GenericRegistry(
            Func<ResilienceStrategyBuilder<TResult>> activator,
            IEqualityComparer<TKey> builderComparer,
            IEqualityComparer<TKey> strategyComparer,
            Func<TKey, string> strategyKeyFormatter,
            Func<TKey, string> builderNameFormatter)
        {
            _activator = activator;
            _builders = new ConcurrentDictionary<TKey, Action<ResilienceStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>>>(builderComparer);
            _strategies = new ConcurrentDictionary<TKey, ResilienceStrategy<TResult>>(strategyComparer);
            _strategyKeyFormatter = strategyKeyFormatter;
            _builderNameFormatter = builderNameFormatter;
        }

        public bool TryAdd(TKey key, ResilienceStrategy<TResult> strategy) => _strategies.TryAdd(key, strategy);

        public bool Remove(TKey key) => _strategies.TryRemove(key, out _);

        public bool TryGet(TKey key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy)
        {
            if (_strategies.TryGetValue(key, out strategy))
            {
                return true;
            }

            if (_builders.TryGetValue(key, out var configure))
            {
                strategy = _strategies.GetOrAdd(key, key =>
                {
                    var context = new ConfigureBuilderContext<TKey>(key, _builderNameFormatter(key), _strategyKeyFormatter(key));
                    return _strategies.GetOrAdd(key, key => new ResilienceStrategy<TResult>(CreateStrategy(_activator, context, configure)));
                });

                return true;
            }

            strategy = null;
            return false;
        }

        public bool TryAddBuilder(TKey key, Action<ResilienceStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>> configure) => _builders.TryAdd(key, configure);

        public bool RemoveBuilder(TKey key) => _builders.TryRemove(key, out _);

        public void Clear() => _strategies.Clear();
    }
}
