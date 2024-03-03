namespace Polly.Registry;

/// <summary>
/// Represents a read-only collection of policies keyed by <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of keys in the policy registry.</typeparam>
public interface IReadOnlyPolicyRegistry<TKey> : IEnumerable<KeyValuePair<TKey, IsPolicy>>
{
    /// <summary>
    /// Gets the <see cref="IsPolicy"/> with the specified key.
    /// <remarks>To retrieve a policy directly as a particular Policy type or Policy interface (avoiding a cast), use the <see cref="Get{TPolicy}"/> method.</remarks>
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The given key was not present in the dictionary.</exception>
    /// <returns>The value associated with the specified key.</returns>
    IsPolicy this[TKey key] { get; }

    /// <summary>
    /// Gets the policy stored under the provided key, casting to <typeparamref name="TPolicy"/>.
    /// </summary>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <returns>The policy stored in the registry under the given key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    TPolicy Get<TPolicy>(TKey key)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Gets the policy stored under the provided key, casting to <typeparamref name="TPolicy"/>.
    /// </summary>
    /// <param name="key">The key of the policy to get.</param>
    /// <param name="policy">
    /// This method returns the policy associated with the specified <paramref name="key"/>, if the
    /// key is found; otherwise null.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <returns>True if Policy exists for the provided Key. False otherwise.</returns>
    bool TryGet<TPolicy>(TKey key, out TPolicy policy)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Total number of policies in the registry.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Determines whether the specified <paramref name="key"/> exists.
    /// </summary>
    /// <param name="key">The Key to locate in the registry</param>
    /// <returns>True if <paramref name="key"/> exists otherwise false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
    bool ContainsKey(TKey key);
}
