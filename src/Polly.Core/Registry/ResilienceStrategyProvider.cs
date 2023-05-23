using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

#pragma warning disable CA1716 // Identifiers should not match keywords

/// <summary>
/// Represents a provider for resilience strategies that are accessible by <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class ResilienceStrategyProvider<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Retrieves a resilience strategy from the provider using the specified key.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <returns>The resilience strategy associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no resilience strategy is found for the specified key.</exception>
    public virtual ResilienceStrategy Get(TKey key)
    {
        if (TryGet(key, out var strategy))
        {
            return strategy;
        }

        throw new KeyNotFoundException($"Unable to find a resilience strategy associated with the key '{key}'. " +
            $"Please ensure that either the resilience strategy or the builder is registered.");
    }

    /// <summary>
    /// Retrieves a generic resilience strategy from the provider using the specified key.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <returns>The resilience strategy associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no resilience strategy is found for the specified key.</exception>
    public virtual ResilienceStrategy<TResult> Get<TResult>(TKey key)
    {
        if (TryGet<TResult>(key, out var strategy))
        {
            return strategy;
        }

        throw new KeyNotFoundException($"Unable to find a generic resilience strategy of '{typeof(TResult).Name}' associated with the key '{key}'. " +
            $"Please ensure that either the generic resilience strategy or the generic builder is registered.");
    }

    /// <summary>
    /// Tries to get a resilience strategy from the provider using the specified key.
    /// </summary>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="strategy">The output resilience strategy if found, null otherwise.</param>
    /// <returns><see langword="true"/> if the strategy was found, <see langword="false"/> otherwise.</returns>
    public abstract bool TryGet(TKey key, [NotNullWhen(true)] out ResilienceStrategy? strategy);

    /// <summary>
    /// Tries to get a generic resilience strategy from the provider using the specified key.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="strategy">The output resilience strategy if found, null otherwise.</param>
    /// <returns><see langword="true"/> if the strategy was found, <see langword="false"/> otherwise.</returns>
    public abstract bool TryGet<TResult>(TKey key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy);
}
