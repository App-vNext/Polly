using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

/// <summary>
/// Represents a registry of resilience pipelines and builders that are accessible by <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <remarks>
/// This class provides a way to organize and manage multiple resilience pipelines
/// using keys of type <typeparamref name="TKey"/>.
/// <para>
/// Additionally, it allows registration of callbacks that configure the pipeline using <see cref="ResiliencePipelineBuilder"/>.
/// These callbacks are called when the resilience pipeline is not yet cached and it's retrieved for the first time.
/// </para>
/// </remarks>
public sealed partial class ResiliencePipelineRegistry<TKey> : ResiliencePipelineProvider<TKey>, IDisposable, IAsyncDisposable
    where TKey : notnull
{
    private readonly Func<ResiliencePipelineBuilder> _activator;
    private readonly ConcurrentDictionary<TKey, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>>> _builders;
    private readonly ConcurrentDictionary<TKey, ResiliencePipeline> _pipelines;
    private readonly ConcurrentDictionary<Type, object> _genericRegistry = new();

    private readonly Func<TKey, string>? _instanceNameFormatter;
    private readonly Func<TKey, string> _builderNameFormatter;
    private readonly IEqualityComparer<TKey> _builderComparer;
    private readonly IEqualityComparer<TKey> _pipelineComparer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResiliencePipelineRegistry{TKey}"/> class with the default comparer.
    /// </summary>
    public ResiliencePipelineRegistry()
        : this(new ResiliencePipelineRegistryOptions<TKey>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResiliencePipelineRegistry{TKey}"/> class with a custom builder factory and comparer.
    /// </summary>
    /// <param name="options">The registry options.</param>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> are <see langword="null"/>.</exception>
    public ResiliencePipelineRegistry(ResiliencePipelineRegistryOptions<TKey> options)
    {
        Guard.NotNull(options);
        Guard.NotNull(options.BuilderFactory);
        Guard.NotNull(options.PipelineComparer);
        Guard.NotNull(options.BuilderComparer);
        Guard.NotNull(options.BuilderNameFormatter);

        _activator = options.BuilderFactory;
        _builders = new ConcurrentDictionary<TKey, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>>>(options.BuilderComparer);
        _pipelines = new ConcurrentDictionary<TKey, ResiliencePipeline>(options.PipelineComparer);
        _instanceNameFormatter = options.InstanceNameFormatter;
        _builderNameFormatter = options.BuilderNameFormatter;
        _builderComparer = options.BuilderComparer;
        _pipelineComparer = options.PipelineComparer;
    }

    /// <inheritdoc/>
    public override bool TryGetPipeline<TResult>(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? pipeline)
    {
        EnsureNotDisposed();

        return GetGenericRegistry<TResult>().TryGet(key, out pipeline);
    }

    /// <inheritdoc/>
    public override bool TryGetPipeline(TKey key, [NotNullWhen(true)] out ResiliencePipeline? pipeline)
    {
        EnsureNotDisposed();

        if (_pipelines.TryGetValue(key, out pipeline))
        {
            return true;
        }

        if (_builders.TryGetValue(key, out var configure))
        {
            pipeline = GetOrAddPipeline(key, configure);
            return true;
        }

        pipeline = null;
        return false;
    }

    /// <summary>
    /// Gets existing pipeline or creates a new one using the <paramref name="configure"/> callback.
    /// </summary>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">The callback that configures the pipeline builder.</param>
    /// <returns>An instance of pipeline.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the registry is already disposed.</exception>
    public ResiliencePipeline GetOrAddPipeline(TKey key, Action<ResiliencePipelineBuilder> configure)
    {
        Guard.NotNull(configure);

        EnsureNotDisposed();

        return GetOrAddPipeline(key, (builder, _) => configure(builder));
    }

    /// <summary>
    /// Gets existing pipeline or creates a new one using the <paramref name="configure"/> callback.
    /// </summary>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">The callback that configures the pipeline builder.</param>
    /// <returns>An instance of pipeline.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the registry is already disposed.</exception>
    public ResiliencePipeline GetOrAddPipeline(TKey key, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>> configure)
    {
        Guard.NotNull(configure);

        EnsureNotDisposed();

        if (_pipelines.TryGetValue(key, out var pipeline))
        {
            return pipeline;
        }

        var componentBuilder = new RegistryPipelineComponentBuilder<ResiliencePipelineBuilder, TKey>(
            _activator,
            key,
            _builderNameFormatter(key),
            _instanceNameFormatter?.Invoke(key),
            configure);

        (var contextPool, var component) = componentBuilder.CreateComponent();

        return _pipelines.GetOrAdd(key, new ResiliencePipeline(component, DisposeBehavior.Reject, contextPool));
    }

    /// <summary>
    /// Gets existing pipeline or creates a new one using the <paramref name="configure"/> callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">The callback that configures the pipeline builder.</param>
    /// <returns>An instance of pipeline.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the registry is already disposed.</exception>
    public ResiliencePipeline<TResult> GetOrAddPipeline<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>> configure)
    {
        Guard.NotNull(configure);

        EnsureNotDisposed();

        return GetOrAddPipeline<TResult>(key, (builder, _) => configure(builder));
    }

    /// <summary>
    /// Gets existing pipeline or creates a new one using the <paramref name="configure"/> callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">The callback that configures the pipeline builder.</param>
    /// <returns>An instance of pipeline.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the registry is already disposed.</exception>
    public ResiliencePipeline<TResult> GetOrAddPipeline<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure)
    {
        Guard.NotNull(configure);

        EnsureNotDisposed();

        return GetGenericRegistry<TResult>().GetOrAdd(key, configure);
    }

    /// <summary>
    /// Tries to add a resilience pipeline builder to the registry.
    /// </summary>
    /// <param name="key">The key used to identify the pipeline builder.</param>
    /// <param name="configure">The action that configures the resilience pipeline builder.</param>
    /// <returns><see langword="true"/> if the builder was added successfully, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// Use this method when you want to create the pipeline on-demand when it's first accessed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the registry is already disposed.</exception>
    public bool TryAddBuilder(TKey key, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>> configure)
    {
        Guard.NotNull(configure);

        EnsureNotDisposed();

        return _builders.TryAdd(key, configure);
    }

    /// <summary>
    /// Tries to add a generic resilience pipeline builder to the registry.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="key">The key used to identify the pipeline builder.</param>
    /// <param name="configure">The action that configures the resilience pipeline builder.</param>
    /// <returns><see langword="true"/> if the builder was added successfully, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// Use this method when you want to create the pipeline on-demand when it's first accessed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the registry is already disposed.</exception>
    public bool TryAddBuilder<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure)
    {
        Guard.NotNull(configure);

        EnsureNotDisposed();

        return GetGenericRegistry<TResult>().TryAddBuilder(key, configure);
    }

    /// <summary>
    /// Disposes all resources that are held by the resilience pipelines created by this builder.
    /// </summary>
    /// <remarks>
    /// After the disposal, all resilience pipelines still used outside of the builder are disposed
    /// and cannot be used anymore.
    /// </remarks>
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    /// <summary>
    /// Disposes all resources that are held by the resilience pipelines created by this builder.
    /// </summary>
    /// <returns>Returns a task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// After the disposal, all resilience pipelines still used outside of the builder are disposed
    /// and cannot be used anymore.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        _disposed = true;

        foreach (var kv in _pipelines)
        {
            await kv.Value.DisposeHelper.ForceDisposeAsync().ConfigureAwait(false);
        }

        foreach (var kv in _genericRegistry)
        {
            await ((IAsyncDisposable)kv.Value).DisposeAsync().ConfigureAwait(false);
        }
    }

    private GenericRegistry<TResult> GetGenericRegistry<TResult>()
    {
        if (_genericRegistry.TryGetValue(typeof(TResult), out var genericRegistry))
        {
            return (GenericRegistry<TResult>)genericRegistry;
        }

        return (GenericRegistry<TResult>)_genericRegistry.GetOrAdd(typeof(TResult), new GenericRegistry<TResult>(
            () => new ResiliencePipelineBuilder<TResult>(_activator()),
            _builderComparer,
            _pipelineComparer,
            _builderNameFormatter,
            _instanceNameFormatter));
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("ResiliencePipelineRegistry", "The resilience pipeline registry has been disposed and cannot be used anymore.");
        }
    }
}
