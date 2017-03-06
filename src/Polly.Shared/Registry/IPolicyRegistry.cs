using System;
using System.Collections.Generic;

namespace Polly.Registry
{
    /// <summary>
    /// Represents a collection of <see cref="Polly.Policy"/> keyed by <typeparamref name="Key"/>.
    /// </summary>
    /// <typeparam name="Key">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="Policy">The type of Policy to store. Must be derived from <see cref="Polly.Policy"/>.</typeparam>
    public interface IPolicyRegistry<Key, Policy> where Policy: Polly.Policy
    {
        /// <summary>
        /// Gets or sets <typeparamref name="Policy"/> with specified Key.
        /// </summary>
        /// <param name="key">The key of the policy to get or set.</param>
        /// <returns>The policy with specified Key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Policy with the specified <paramref name="key"/> is not found in the registry.</exception>
        Policy this[Key key] { get; set; }

        /// <summary>
        /// Total number of policies in the registry.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a <typeparamref name="Policy"/> with the provided key to the registry.
        /// </summary>
        /// <param name="key">The key of the <typeparamref name="Policy"/> to add.</param>
        /// <param name="value">The <typeparamref name="Policy"/> to store in the registry.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists.</exception>
        void Add(Key key, Policy value);

        /// <summary>
        /// Determines whether the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">The Key to locate in the registry</param>
        /// <returns>True if <paramref name="key"/> exists otherwise false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        bool ContainsKey(Key key);

        /// <summary>
        /// Removes the specified <typeparamref name="Policy"/> from the registry.
        /// </summary>
        /// <param name="key">The key of the policy to remove.</param>
        /// <returns>True if <see cref="Polly.Policy"/> is successfully removed. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        bool Remove(Key key);

        /// <summary>
        /// Gets the <typeparamref name="Policy"/> associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// This method returns the <typeparamref name="Policy"/> associated with the specified <paramref name="key"/>, if the
        /// key is found; otherwise null.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>True if Policy exists for the provided Key. False otherwise.</returns>
        bool TryGetValue(Key key, out Policy value);

        /// <summary>
        /// Removes all keys and policies from registry.
        /// </summary>
        void Clear();
    }
}
