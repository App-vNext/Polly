using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public sealed partial class ResiliencePipelineRegistry<TKey> : ResiliencePipelineProvider<TKey>
    where TKey : notnull
{
    private sealed class GenericRegistry<TResult> : IAsyncDisposable
    {
        private readonly Func<ResiliencePipelineBuilder<TResult>> _activator;
        private readonly ConcurrentDictionary<TKey, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>>> _builders;
        private readonly ConcurrentDictionary<TKey, ResiliencePipeline<TResult>> _pipelines;

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
            _pipelines = new ConcurrentDictionary<TKey, ResiliencePipeline<TResult>>(strategyComparer);
            _builderNameFormatter = builderNameFormatter;
            _instanceNameFormatter = instanceNameFormatter;
        }

        public bool TryGet(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? strategy)
        {
            if (_pipelines.TryGetValue(key, out strategy))
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
            if (_pipelines.TryGetValue(key, out var pipeline))
            {
                return pipeline;
            }

            var componentBuilder = new RegistryPipelineComponentBuilder<ResiliencePipelineBuilder<TResult>, TKey>(
                _activator,
                key,
                _builderNameFormatter(key),
                _instanceNameFormatter?.Invoke(key),
                configure);

            (var contextPool, var component) = componentBuilder.CreateComponent();

            return _pipelines.GetOrAdd(key, new ResiliencePipeline<TResult>(component, DisposeBehavior.Reject, contextPool));
        }

        public bool TryAddBuilder(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure) => _builders.TryAdd(key, configure);

        public async ValueTask DisposeAsync()
        {
            foreach (var kv in _pipelines)
            {
                await kv.Value.DisposeHelper.ForceDisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
