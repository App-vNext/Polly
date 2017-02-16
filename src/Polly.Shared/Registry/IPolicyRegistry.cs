using System;
using System.Collections.Generic;

namespace Polly.Registry
{
    /// <summary>
    /// Represents a collection of <see cref="Polly.Policy"/> keyed by <typeparamref name="Key"/>
    /// </summary>
    /// <typeparam name="Key">The type of keys in the dictionary</typeparam>
    /// <typeparam name="Policy">The type of Policy to store. Must have <see cref="Polly.Policy"/> as base class. </typeparam>
    public interface IPolicyRegistry<Key, Policy> where Policy: Polly.Policy
    {
        /// <summary>
        /// Gets or sets policy with specified Key.
        /// </summary>
        /// <param name="key">The key of the policy to get or set.</param>
        /// <returns>The policy with specified Key.</returns>
        /// <exception cref="System.ArgumentNullException">Key is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and key is not found.</exception>
        Policy this[Key key] { get; set; }

        /// <summary>
        /// Total number of policies in the registry
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds an element with the provided key and <see cref="Polly.Policy"/> to the registry.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The <see cref="Polly.Policy"/> to store in the registry.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists</exception>
        void Add(Key key, Policy value);

        /// <summary>
        /// Determines whether the specified Key exists.
        /// </summary>
        /// <param name="key">The Key to locate in the registry</param>
        /// <returns>True if Key exists otherwise false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        bool ContainsKey(Key key);

        /// <summary>
        /// Removes the specified <see cref="Polly.Policy"/> from the registry
        /// </summary>
        /// <param name="key">The key of the policy to remove</param>
        /// <returns>True if Policy is successfully removed. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        bool Remove(Key key);

        /// <summary>
        /// Gets the Policy associated with the specified key
        /// </summary>
        /// <param name="key">The key whose value to get</param>
        /// <param name="value">
        /// When this method returns, the Policy associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>True if Policy exists for the provided Key. False otherwise</returns>
        bool TryGetValue(Key key, out Policy value);

        /// <summary>
        /// Removes all keys and values from registry
        /// </summary>
        void Clear();
    }
}
