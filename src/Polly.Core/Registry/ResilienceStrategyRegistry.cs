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
public sealed class ResilienceStrategyRegistry<TKey> : ResilienceStrategyProvider<TKey>
    where TKey : notnull
{
    private readonly Func<ResilienceStrategyBuilder> _activator;
    private readonly ConcurrentDictionary<TKey, Action<TKey, ResilienceStrategyBuilder>> _builders;
    private readonly ConcurrentDictionary<TKey, ResilienceStrategy> _strategies;
    private readonly Func<TKey, string> _keyFormatter;

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
    public ResilienceStrategyRegistry(ResilienceStrategyRegistryOptions<TKey> options)
    {
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The resilience strategy registry options are invalid.");

        _activator = options.BuilderFactory;
        _builders = new ConcurrentDictionary<TKey, Action<TKey, ResilienceStrategyBuilder>>(options.BuilderComparer);
        _strategies = new ConcurrentDictionary<TKey, ResilienceStrategy>(options.StrategyComparer);
        _keyFormatter = options.KeyFormatter;
    }

    /// <summary>
    /// Tries to add an existing resilience strategy to the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="strategy">The resilience strategy instance.</param>
    /// <returns>true if the strategy was added successfully, false otherwise.</returns>
    public bool TryAdd(TKey key, ResilienceStrategy strategy)
    {
        Guard.NotNull(strategy);

        return _strategies.TryAdd(key, strategy);
    }

    /// <summary>
    /// Removes a resilience strategy from the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <returns>true if the strategy was removed successfully, false otherwise.</returns>
    public bool Remove(TKey key) => _strategies.TryRemove(key, out _);

    /// <summary>
    /// Tries to get a resilience strategy from the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="strategy">The output resilience strategy if found, null otherwise.</param>
    /// <returns>true if the strategy was found, false otherwise.</returns>
    /// <remarks>
    /// Tries to get a strategy using the given key. If not found, it looks for a builder with the key, builds the strategy,
    /// adds it to the registry, and returns true. If neither the strategy nor the builder is found, the method returns false.
    /// </remarks>
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
                builder.Properties.Set(TelemetryUtil.StrategyKey, _keyFormatter(key));
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
    /// <returns>True if the builder was added successfully, false otherwise.</returns>
    /// <remarks>
    /// Use this method when you want to create the strategy on-demand when it's first accessed.
    /// </remarks>
    public bool TryAddBuilder(TKey key, Action<TKey, ResilienceStrategyBuilder> configure)
    {
        Guard.NotNull(configure);

        return _builders.TryAdd(key, configure);
    }

    /// <summary>
    /// Removes a resilience strategy builder from the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy builder.</param>
    /// <returns>true if the builder was removed successfully, false otherwise.</returns>
    public bool RemoveBuilder(TKey key) => _builders.TryRemove(key, out _);

    /// <summary>
    /// Clears all cached strategies.
    /// </summary>
    /// <remarks>
    /// This method only clears the cached strategies, the registered builders are kept unchanged.
    /// </remarks>
    public void Clear() => _strategies.Clear();
}
