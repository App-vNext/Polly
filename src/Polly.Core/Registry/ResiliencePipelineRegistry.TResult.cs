using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public sealed partial class ResiliencePipelineRegistry<TKey> : ResiliencePipelineProvider<TKey>
    where TKey : notnull
{
    private sealed class GenericRegistry<TResult>
    {
        private readonly Func<ResiliencePipelineBuilder<TResult>> _activator;
        private readonly ConcurrentDictionary<TKey, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>>> _builders;
        private readonly ConcurrentDictionary<TKey, ResiliencePipeline<TResult>> _strategies;

        private readonly Func<TKey, string> _builderNameFormatter;
        private readonly Func<TKey, string>? _instanceNameFormatter;

        public GenericRegistry(
            Func<ResiliencePipelineBuilder<TResult>> activator,
            IEqualityComparer<TKey> builderComparer,
            IEqualityComparer<TKey> strategyComparer,
            Func<TKey, string> builderNameFormatter,
            Func<TKey, string>? instanceNameFormatter)
        {
            _activator = activator;
            _builders = new ConcurrentDictionary<TKey, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>>>(builderComparer);
            _strategies = new ConcurrentDictionary<TKey, ResiliencePipeline<TResult>>(strategyComparer);
            _builderNameFormatter = builderNameFormatter;
            _instanceNameFormatter = instanceNameFormatter;
        }

        public bool TryAdd(TKey key, ResiliencePipeline<TResult> strategy) => _strategies.TryAdd(key, strategy);

        public bool Remove(TKey key) => _strategies.TryRemove(key, out _);

        public bool TryGet(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? strategy)
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

        public ResiliencePipeline<TResult> GetOrAdd(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure)
        {
            var context = new ConfigureBuilderContext<TKey>(key, _builderNameFormatter(key), _instanceNameFormatter?.Invoke(key));

#if NETCOREAPP3_0_OR_GREATER
            return _strategies.GetOrAdd(key, static (_, factory) =>
            {
                return new ResiliencePipeline<TResult>(CreatePipeline(factory.instance._activator, factory.context, factory.configure));
            },
            (instance: this, context, configure));
#else
            return _strategies.GetOrAdd(key, _ => new ResiliencePipeline<TResult>(CreatePipeline(_activator, context, configure)));
#endif
        }

        public bool TryAddBuilder(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure) => _builders.TryAdd(key, configure);

        public bool RemoveBuilder(TKey key) => _builders.TryRemove(key, out _);

        public void Clear() => _strategies.Clear();
    }
}
