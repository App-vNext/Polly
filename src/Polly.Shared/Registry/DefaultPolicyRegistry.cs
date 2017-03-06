using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

namespace Polly.Registry
{
    /// <summary>
    /// Stores a registry of <see cref="System.String"/> and <see cref="Policy"/> pair.
    /// </summary>
    /// <remarks>Uses ConcurrentDictionary to store the collection.</remarks>
    public class DefaultPolicyRegistry : IPolicyRegistry<string, Policy>
    {
        private IDictionary<string, Policy> _registry = new ConcurrentDictionary<string, Policy>();

        /// <summary>
        /// Gets or sets the <see cref="Policy"/> associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the <see cref="Policy"/> to get or set.</param>
        /// <returns>The <see cref="Policy"/> with specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Policy with the specified <paramref name="key"/> does not exists in the registry.</exception>
        public Policy this[string key]
        {
            get { return _registry[key]; }

            set { _registry[key] = value; }
        }

        /// <summary>
        /// Total number of policies in the registry.
        /// </summary>
        public int Count => _registry.Count;

        /// <summary>
        /// Adds an element with the provided key and <see cref="Policy"/> to the registry.
        /// </summary>
        /// <param name="key">The string to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="Policy"/> to store in the registry.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists.</exception>
        public void Add(string key, Policy value) =>
            _registry.Add(key, value);

        /// <summary>
        /// Removes all keys and policies from registry.
        /// </summary>
        public void Clear() =>
            _registry.Clear();

        /// <summary>
        /// Determines whether the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">The key to locate in the registry.</param>
        /// <returns>True if <paramref name="key"/> exists otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(string key) =>
            _registry.ContainsKey(key);

        /// <summary>
        /// Removes the stored <see cref="Policy"/> under specified <paramref name="key"/> from the registry.
        /// </summary>
        /// <param name="key">The <paramref name="key"/> of the policy to remove.</param>
        /// <returns>True if <see cref="Policy"/> is successfully removed. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool Remove(string key) =>
            _registry.Remove(key);

        /// <summary>
        /// Gets the <see cref="Policy"/> associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// This method returns the <see cref="Policy"/> associated with the specified <paramref name="key"/>, if the
        /// key is found; otherwise null.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>True if Policy exists for the provided Key. False otherwise.</returns>
        public bool TryGetValue(string key, out Policy value) =>
            _registry.TryGetValue(key, out value);
    }
}
