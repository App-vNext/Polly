using System;
using System.Collections.Generic;

namespace Polly.Registry
{
    /// <summary>
    /// Represents a collection of <see cref="Polly.Policy"/> keyed by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TPolicy">The type of Policy to store. Must be derived from <see cref="Polly.Policy"/>.</typeparam>
    public interface IPolicyRegistry<TKey, TPolicy> where TPolicy: Polly.Policy
    {
        /// <summary>
        /// Gets or sets <typeparamref name="TPolicy"/> with specified Key.
        /// </summary>
        /// <param name="key">The key of the policy to get or set.</param>
        /// <returns>The policy with specified Key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Policy with the specified <paramref name="key"/> is not found in the registry.</exception>
        TPolicy this[TKey key] { get; set; }

        /// <summary>
        /// Total number of policies in the registry.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a <typeparamref name="TPolicy"/> with the provided key to the registry.
        /// </summary>
        /// <param name="key">The key of the <typeparamref name="TPolicy"/> to add.</param>
        /// <param name="value">The <typeparamref name="TPolicy"/> to store in the registry.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists.</exception>
        void Add(TKey key, TPolicy value);

        /// <summary>
        /// Determines whether the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">The Key to locate in the registry</param>
        /// <returns>True if <paramref name="key"/> exists otherwise false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Removes the specified <typeparamref name="TPolicy"/> from the registry.
        /// </summary>
        /// <param name="key">The key of the policy to remove.</param>
        /// <returns>True if <see cref="Polly.Policy"/> is successfully removed. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        bool Remove(TKey key);

        /// <summary>
        /// Gets the <typeparamref name="TPolicy"/> associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// This method returns the <typeparamref name="TPolicy"/> associated with the specified <paramref name="key"/>, if the
        /// key is found; otherwise null.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>True if Policy exists for the provided Key. False otherwise.</returns>
        bool TryGetValue(TKey key, out TPolicy value);

        /// <summary>
        /// Removes all keys and policies from registry.
        /// </summary>
        void Clear();
    }
}
