using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Polly;

namespace Polly.Registry
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public sealed class DefaultPolicyRegistry : IPolicyRegistry<string, Policy>
    {
        private Dictionary<string, Policy> _registry = new Dictionary<string, Policy>();

        public Policy this[string key] { get => _registry[key]; set => _registry[key] = value; }

        public ICollection<string> Keys => _registry.Keys;

        public ICollection<Policy> Values => _registry.Values;

        public int Count => _registry.Count;

        public void Add(string key, Policy value) =>
            _registry.Add(key, value);

        public void Clear() =>
            _registry.Clear();

        public bool ContainsKey(string key) =>
            _registry.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, Policy>> GetEnumerator() =>
            _registry.GetEnumerator();

        public bool Remove(string key) =>
            _registry.Remove(key);

        public bool TryGetValue(string key, out Policy value) =>
            _registry.TryGetValue(key, out value);

        #region IEnumerable members
        IEnumerator IEnumerable.GetEnumerator() =>
            (_registry as IEnumerable).GetEnumerator();
        #endregion

        #region ICollection members
        public bool IsReadOnly =>
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
