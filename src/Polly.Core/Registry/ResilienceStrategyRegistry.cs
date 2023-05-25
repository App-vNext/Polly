using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Telemetry;

namespace Polly.Registry;

/// <summary>
/// Represents a registry of resilience strategies and builders that are accessible by <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <remarks>
/// This class provides a way to organize and manage multiple resilience strategies
/// using keys of type <typeparamref name="TKey"/>.
/// <para>
/// Additionally, it allows registration of callbacks that configure the strategy using <see cref="ResilienceStrategyBuilder"/>.
/// These callbacks are called when the resilience strategy is not yet cached and it's retrieved for the first time.
/// </para>
/// </remarks>
public sealed partial class ResilienceStrategyRegistry<TKey> : ResilienceStrategyProvider<TKey>
    where TKey : notnull
{
    private readonly Func<ResilienceStrategyBuilder> _activator;
    private readonly ConcurrentDictionary<TKey, Action<TKey, ResilienceStrategyBuilder>> _builders;
    private readonly ConcurrentDictionary<TKey, ResilienceStrategy> _strategies;
    private readonly ConcurrentDictionary<Type, object> _genericRegistry = new();

    private readonly Func<TKey, string> _strategyKeyFormatter;
    private readonly Func<TKey, string> _builderNameFormatter;
    private readonly IEqualityComparer<TKey> _builderComparer;
    private readonly IEqualityComparer<TKey> _strategyComparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyRegistry{TKey}"/> class with the default comparer.
    /// </summary>
    public ResilienceStrategyRegistry()
        : this(new ResilienceStrategyRegistryOptions<TKey>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyRegistry{TKey}"/> class with a custom builder factory and comparer.
    /// </summary>
    /// <param name="options">The registry options.</param>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> are <see langword="null"/>.</exception>
    public ResilienceStrategyRegistry(ResilienceStrategyRegistryOptions<TKey> options)
    {
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The resilience strategy registry options are invalid.");

        _activator = options.BuilderFactory;
        _builders = new ConcurrentDictionary<TKey, Action<TKey, ResilienceStrategyBuilder>>(options.BuilderComparer);
        _strategies = new ConcurrentDictionary<TKey, ResilienceStrategy>(options.StrategyComparer);
        _strategyKeyFormatter = options.StrategyKeyFormatter;
        _builderNameFormatter = options.BuilderNameFormatter;
        _builderComparer = options.BuilderComparer;
        _strategyComparer = options.StrategyComparer;
    }

    /// <summary>
    /// Tries to add an existing resilience strategy to the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="strategy">The resilience strategy instance.</param>
    /// <returns><see langword="true"/> if the strategy was added successfully, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public bool TryAdd(TKey key, ResilienceStrategy strategy)
    {
        Guard.NotNull(strategy);

        return _strategies.TryAdd(key, strategy);
    }

    /// <summary>
    /// Tries to add an existing generic resilience strategy to the registry.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="strategy">The resilience strategy instance.</param>
    /// <returns><see langword="true"/> if the strategy was added successfully, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public bool TryAdd<TResult>(TKey key, ResilienceStrategy<TResult> strategy)
    {
        Guard.NotNull(strategy);

        return GetGenericRegistry<TResult>().TryAdd(key, strategy);
    }

    /// <summary>
    /// Removes a resilience strategy from the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <returns><see langword="true"/> if the strategy was removed successfully, <see langword="false"/> otherwise.</returns>
    public bool Remove(TKey key) => _strategies.TryRemove(key, out _);

    /// <summary>
    /// Removes a generic resilience strategy from the registry.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <returns><see langword="true"/> if the strategy was removed successfully, <see langword="false"/> otherwise.</returns>
    public bool Remove<TResult>(TKey key) => GetGenericRegistry<TResult>().Remove(key);

    /// <inheritdoc/>
    public override bool TryGet<TResult>(TKey key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy)
    {
        return GetGenericRegistry<TResult>().TryGet(key, out strategy);
    }

    /// <inheritdoc/>
    public override bool TryGet(TKey key, [NotNullWhen(true)] out ResilienceStrategy? strategy)
    {
        if (_strategies.TryGetValue(key, out strategy))
        {
            return true;
        }

        if (_builders.TryGetValue(key, out var configure))
        {
            strategy = _strategies.GetOrAdd(key, key =>
            {
                var builder = _activator();
                builder.BuilderName = _builderNameFormatter(key);
                builder.Properties.Set(TelemetryUtil.StrategyKey, _strategyKeyFormatter(key));
                configure(key, builder);
                return builder.Build();
            });

            return true;
        }

        strategy = null;
        return false;
    }

    /// <summary>
    /// Tries to add a resilience strategy builder to the registry.
    /// </summary>
    /// <param name="key">The key used to identify the strategy builder.</param>
    /// <param name="configure">The action that configures the resilience strategy builder.</param>
    /// <returns><see langword="true"/> if the builder was added successfully, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// Use this method when you want to create the strategy on-demand when it's first accessed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    public bool TryAddBuilder(TKey key, Action<TKey, ResilienceStrategyBuilder> configure)
    {
        Guard.NotNull(configure);

        return _builders.TryAdd(key, configure);
    }

    /// <summary>
    /// Tries to add a generic resilience strategy builder to the registry.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="key">The key used to identify the strategy builder.</param>
    /// <param name="configure">The action that configures the resilience strategy builder.</param>
    /// <returns><see langword="true"/> if the builder was added successfully, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// Use this method when you want to create the strategy on-demand when it's first accessed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    public bool TryAddBuilder<TResult>(TKey key, Action<TKey, ResilienceStrategyBuilder<TResult>> configure)
    {
        Guard.NotNull(configure);

        return GetGenericRegistry<TResult>().TryAddBuilder(key, configure);
    }

    /// <summary>
    /// Removes a resilience strategy builder from the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy builder.</param>
    /// <returns><see langword="true"/> if the builder was removed successfully, <see langword="false"/> otherwise.</returns>
    public bool RemoveBuilder(TKey key) => _builders.TryRemove(key, out _);

    /// <summary>
    /// Removes a generic resilience strategy builder from the registry.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="key">The key used to identify the resilience strategy builder.</param>
    /// <returns><see langword="true"/> if the builder was removed successfully, <see langword="false"/> otherwise.</returns>
    public bool RemoveBuilder<TResult>(TKey key) => GetGenericRegistry<TResult>().RemoveBuilder(key);

    /// <summary>
    /// Clears all cached strategies.
    /// </summary>
    /// <remarks>
    /// This method only clears the cached strategies, the registered builders are kept unchanged.
    /// </remarks>
    public void Clear() => _strategies.Clear();

    /// <summary>
    /// Clears all cached generic strategies.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <remarks>
    /// This method only clears the cached strategies, the registered builders are kept unchanged.
    /// </remarks>
    public void Clear<TResult>() => GetGenericRegistry<TResult>().Clear();

    private GenericRegistry<TResult> GetGenericRegistry<TResult>()
    {
        if (_genericRegistry.TryGetValue(typeof(TResult), out var genericRegistry))
        {
            return (GenericRegistry<TResult>)genericRegistry;
        }

        return (GenericRegistry<TResult>)_genericRegistry.GetOrAdd(typeof(TResult), _ =>
        {
            return new GenericRegistry<TResult>(
                () => new ResilienceStrategyBuilder<TResult>(_activator()),
                _builderComparer,
                _strategyComparer,
                _strategyKeyFormatter,
                _builderNameFormatter);
        });
    }
}
