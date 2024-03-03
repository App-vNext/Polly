namespace Polly.Registry;

/// <summary>
/// Represents a collection of policies keyed by <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of keys in the policy registry.</typeparam>
public interface IPolicyRegistry<TKey> : IReadOnlyPolicyRegistry<TKey>
{
    /// <summary>
    /// Adds an element with the provided key and policy to the registry.
    /// </summary>
    /// <param name="key">The key for the policy.</param>
    /// <param name="policy">The policy to store in the registry.</param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists.</exception>
    void Add<TPolicy>(TKey key, TPolicy policy)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Gets or sets the <see cref="IsPolicy"/> with the specified key.
    /// <remarks>To retrieve a policy directly as a particular Policy type or Policy interface (avoiding a cast), use the <see cref="IReadOnlyPolicyRegistry{TKey}.Get{TPolicy}"/> method.</remarks>
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The given key was not present in the dictionary.</exception>
    /// <returns>The value associated with the specified key.</returns>
    new IsPolicy this[TKey key] { get; set; }

    /// <summary>
    /// Removes the policy stored under the specified <paramref name="key"/> from the registry.
    /// </summary>
    /// <param name="key">The key of the policy to remove.</param>
    /// <returns>True if the policy is successfully removed. Otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    bool Remove(TKey key);

    /// <summary>
    /// Removes all keys and policies from registry.
    /// </summary>
    void Clear();
}
