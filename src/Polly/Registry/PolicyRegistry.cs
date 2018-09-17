using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Polly.Registry
{
    /// <summary>
    /// Stores a registry of <see cref="System.String"/> and <see cref="Polly.Policy"/> pair.
    /// </summary>
    /// <remarks>Uses ConcurrentDictionary to store the collection.</remarks>
    public class PolicyRegistry : IPolicyRegistry<string>
    {
        private readonly IDictionary<string, IsPolicy> _registry = new ConcurrentDictionary<string, IsPolicy>();

        /// <summary>
        /// Total number of policies in the registry.
        /// </summary>
        public int Count => _registry.Count;

        /// <summary>
        /// Adds an element with the provided key and policy to the registry.
        /// </summary>
        /// <param name="key">The key for the policy.</param>
        /// <param name="policy">The policy to store in the registry.</param>
        /// <typeparam name="TPolicy">The type of Policy.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists.</exception>
        public void Add<TPolicy>(string key, TPolicy policy) where TPolicy : IsPolicy
        {
            _registry.Add(key, policy);
        }

        /// <summary>
        /// Gets of sets the <see cref="IsPolicy"/> with the specified key.
        /// <remarks>To retrieve a policy directly as a particular Policy type or Policy interface (avoiding a cast), use the <see cref="Get{TPolicy}"/> method.</remarks>
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="KeyNotFoundException">The given key was not present in the dictionary.</exception>
        /// <returns>The value associated with the specified key.</returns>
        public IsPolicy this[string key]
        {
            get
            {
                return _registry[key];
            }
            set
            {
                _registry[key] = value;
            }
        }

        /// <summary>
        /// Gets the policy stored under the provided key, casting to <typeparamref name="TPolicy"/>.
        /// </summary>
        /// <typeparam name="TPolicy">The type of Policy.</typeparam>
        /// <returns>The policy stored in the registry under the given key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">The given key was not present in the dictionary.</exception>
        public TPolicy Get<TPolicy>(string key) where TPolicy : IsPolicy
        {
            return (TPolicy) _registry[key];
        }

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
        public bool TryGet<TPolicy>(string key, out TPolicy policy) where TPolicy : IsPolicy
        {
            IsPolicy value;
            bool got = _registry.TryGetValue(key, out value);
            policy = got ? (TPolicy)value : default(TPolicy);
            return got;
        }

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
        /// Removes the stored <see cref="Polly.Policy"/> under specified <paramref name="key"/> from the registry.
        /// </summary>
        /// <param name="key">The <paramref name="key"/> of the policy to remove.</param>
        /// <returns>True if <see cref="Polly.Policy"/> is successfully removed. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool Remove(string key) =>
            _registry.Remove(key);
    }
}
