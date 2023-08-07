using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public sealed partial class ResilienceStrategyRegistry<TKey> : ResilienceStrategyProvider<TKey>
    where TKey : notnull
{
    private sealed class GenericRegistry<TResult>
    {
        private readonly Func<CompositeStrategyBuilder<TResult>> _activator;
        private readonly ConcurrentDictionary<TKey, Action<CompositeStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>>> _builders;
        private readonly ConcurrentDictionary<TKey, ResilienceStrategy<TResult>> _strategies;

        private readonly Func<TKey, string> _builderNameFormatter;
        private readonly Func<TKey, string>? _instanceNameFormatter;

        public GenericRegistry(
            Func<CompositeStrategyBuilder<TResult>> activator,
            IEqualityComparer<TKey> builderComparer,
            IEqualityComparer<TKey> strategyComparer,
            Func<TKey, string> builderNameFormatter,
            Func<TKey, string>? instanceNameFormatter)
        {
            _activator = activator;
            _builders = new ConcurrentDictionary<TKey, Action<CompositeStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>>>(builderComparer);
            _strategies = new ConcurrentDictionary<TKey, ResilienceStrategy<TResult>>(strategyComparer);
            _builderNameFormatter = builderNameFormatter;
            _instanceNameFormatter = instanceNameFormatter;
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
                strategy = GetOrAdd(key, configure);
                return true;
            }

            strategy = null;
            return false;
        }

        public ResilienceStrategy<TResult> GetOrAdd(TKey key, Action<CompositeStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>> configure)
        {
            var context = new ConfigureBuilderContext<TKey>(key, _builderNameFormatter(key), _instanceNameFormatter?.Invoke(key));

#if NETCOREAPP3_0_OR_GREATER
            return _strategies.GetOrAdd(key, static (_, factory) =>
            {
                return CreateStrategy(
                    factory.instance._activator,
                    factory.context,
                    (builder, context) => factory.configure((CompositeStrategyBuilder<TResult>)builder, context));
            },
            (instance: this, context, configure));
#else
            return _strategies.GetOrAdd(key, _ =>
            {
                return CreateStrategy(
                    _activator,
                    context,
                    (builder, context) => configure((CompositeStrategyBuilder<TResult>)builder, context));
            });
#endif
        }

        public bool TryAddBuilder(TKey key, Action<CompositeStrategyBuilder<TResult>, ConfigureBuilderContext<TKey>> configure) => _builders.TryAdd(key, configure);

        public bool RemoveBuilder(TKey key) => _builders.TryRemove(key, out _);

        public void Clear() => _strategies.Clear();
    }
}
