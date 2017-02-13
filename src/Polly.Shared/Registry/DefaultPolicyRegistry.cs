using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

namespace Polly.Registry
{
    /// <summary>
    /// Stores a collection of <see cref="System.String"/> and <see cref="Policy"/> pair.
    /// </summary>
    /// <remarks>Uses ConcurrentDictionary to store the collection.</remarks>
    public class DefaultPolicyRegistry : IPolicyRegistry<string, Policy>
    {
        private IDictionary<string, Policy> _registry = new ConcurrentDictionary<string, Policy>();

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        /// <exception cref="ArgumentNullException">Key is null</exception>
        /// <exception cref="KeyNotFoundException">Key does not exist in the collection.</exception>
        public Policy this[string key]
        {
            get { return _registry[key]; }

            set { _registry[key] = value; }
        }

        /// <summary>
        /// Gets a collection containing the keys.
        /// </summary>
        public ICollection<string> Keys => _registry.Keys;

        /// <summary>
        /// Gets a collection of stored <see cref="Policy"/> .
        /// </summary>
        public ICollection<Policy> Values => _registry.Values;

        /// <summary>
        /// Gets the number of key/<see cref="Policy"/> pairs stored in the registry.
        /// </summary>
        public int Count => _registry.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, Policy value) =>
            _registry.Add(key, value);

        /// <summary>
        /// Removes all keys and values from registry
        /// </summary>
        public void Clear() =>
            _registry.Clear();

        /// <summary>
        /// Determines whether the specified Key exists.
        /// </summary>
        /// <param name="key">The Key to locate in the registry</param>
        /// <returns>True if Key exists otherwise false</returns>
        public bool ContainsKey(string key) =>
            _registry.ContainsKey(key);

        /// <summary>
        /// Returns an enumerator that iterates through the registry
        /// </summary>
        /// <returns>An enumerator for <see cref="ConcurrentDictionary{String, Policy}"/></returns>
        public IEnumerator<KeyValuePair<string, Policy>> GetEnumerator() =>
            _registry.GetEnumerator();

        /// <summary>
        /// Removes the specified <see cref="Policy"/>  from the registry
        /// </summary>
        /// <param name="key">The key of the policy to remove</param>
        /// <returns>True if Policy is successfully removed. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool Remove(string key) =>
            _registry.Remove(key);

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
        public bool TryGetValue(string key, out Policy value) =>
            _registry.TryGetValue(key, out value);

        #region IEnumerable members
        IEnumerator IEnumerable.GetEnumerator() =>
            (_registry as IEnumerable).GetEnumerator();
        #endregion

        #region ICollection members
        bool ICollection<KeyValuePair<string, Policy>>.IsReadOnly =>
            (_registry as ICollection<KeyValuePair<string, Policy>>).IsReadOnly;

        void ICollection<KeyValuePair<string, Policy>>.Add(KeyValuePair<string, Policy> item) =>
            (_registry as ICollection<KeyValuePair<string, Policy>>).Add(item);

        bool ICollection<KeyValuePair<string, Policy>>.Contains(KeyValuePair<string, Policy> item) =>
            (_registry as ICollection<KeyValuePair<string, Policy>>).Contains(item);

        void ICollection<KeyValuePair<string, Policy>>.CopyTo(KeyValuePair<string, Policy>[] array, int arrayIndex) =>
            (_registry as ICollection<KeyValuePair<string, Policy>>).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<string, Policy>>.Remove(KeyValuePair<string, Policy> item) =>
            (_registry as ICollection<KeyValuePair<string, Policy>>).Remove(item);
        #endregion
    }
}
