namespace Polly;

/// <summary>
/// Context that carries with a single execution through a Policy.   Commonly-used properties are directly on the class.  Backed by a dictionary of string key / object value pairs, to which user-defined values may be added.
/// <remarks>Do not re-use an instance of <see cref="Context"/> across more than one execution.</remarks>
/// </summary>
public partial class Context : IDictionary<string, object>, IDictionary, IReadOnlyDictionary<string, object>
{
    // For an individual execution through a policy or policywrap, it is expected that all execution steps (for example executing the user delegate, invoking policy-activity delegates such as onRetry, onBreak, onTimeout etc) execute sequentially.
    // Therefore, this class is intentionally not constructed to be safe for concurrent access from multiple threads.

    private Dictionary<string, object> _wrappedDictionary;

    private Dictionary<string, object> WrappedDictionary => _wrappedDictionary ?? (_wrappedDictionary = []);

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class, with the specified <paramref name="operationKey" /> and the supplied <paramref name="contextData"/>.
    /// </summary>
    /// <param name="operationKey">The operation key.</param>
    /// <param name="contextData">The context data.</param>
    public Context(string operationKey, IDictionary<string, object> contextData)
        : this(contextData) =>
        OperationKey = operationKey;

    internal Context(IDictionary<string, object> contextData)
        : this()
    {
        if (contextData == null)
        {
            throw new ArgumentNullException(nameof(contextData));
        }

        _wrappedDictionary = new Dictionary<string, object>(contextData);
    }

    #region IDictionary<string,object> implementation

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public ICollection<string> Keys => WrappedDictionary.Keys;

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public ICollection<object> Values => WrappedDictionary.Values;

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public int Count => WrappedDictionary.Count;

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => ((IDictionary<string, object>)WrappedDictionary).IsReadOnly;

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public object this[string key]
    {
        get => WrappedDictionary[key];
        set => WrappedDictionary[key] = value;
    }

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public void Add(string key, object value) =>
        WrappedDictionary.Add(key, value);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public bool ContainsKey(string key) =>
        WrappedDictionary.ContainsKey(key);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public bool Remove(string key) =>
        WrappedDictionary.Remove(key);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public bool TryGetValue(string key, out object value) =>
        WrappedDictionary.TryGetValue(key, out value);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) =>
        ((IDictionary<string, object>)WrappedDictionary).Add(item);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public void Clear() =>
        WrappedDictionary.Clear();

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) =>
        ((IDictionary<string, object>)WrappedDictionary).Contains(item);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
        ((IDictionary<string, object>)WrappedDictionary).CopyTo(array, arrayIndex);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) =>
        ((IDictionary<string, object>)WrappedDictionary).Remove(item);

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        WrappedDictionary.GetEnumerator();

    /// <inheritdoc cref="IDictionary{TKey,Value}"/>
    IEnumerator IEnumerable.GetEnumerator() =>
        WrappedDictionary.GetEnumerator();

    /// <inheritdoc cref="IDictionary"/>
    public void Add(object key, object value) =>
        ((IDictionary)WrappedDictionary).Add(key, value);

    /// <inheritdoc cref="IDictionary"/>
    public bool Contains(object key) =>
        ((IDictionary)WrappedDictionary).Contains(key);

    /// <inheritdoc cref="IDictionary"/>
    IDictionaryEnumerator IDictionary.GetEnumerator() =>
        ((IDictionary)WrappedDictionary).GetEnumerator();

    /// <inheritdoc cref="IDictionary"/>
    public void Remove(object key) =>
        ((IDictionary)WrappedDictionary).Remove(key);

    /// <inheritdoc cref="IDictionary"/>
    public void CopyTo(Array array, int index) =>
        ((IDictionary)WrappedDictionary).CopyTo(array, index);

    #endregion

    #region IReadOnlyDictionary<string, object> implementation

    /// <inheritdoc/>
    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => ((IReadOnlyDictionary<string, object>)WrappedDictionary).Keys;

    /// <inheritdoc/>
    IEnumerable<object> IReadOnlyDictionary<string, object>.Values => ((IReadOnlyDictionary<string, object>)WrappedDictionary).Values;

    #endregion

    #region IDictionary implementation

    /// <inheritdoc cref="IDictionary"/>
    bool IDictionary.IsFixedSize => ((IDictionary)WrappedDictionary).IsFixedSize;

    /// <inheritdoc cref="IDictionary"/>
    bool IDictionary.IsReadOnly => ((IDictionary)WrappedDictionary).IsReadOnly;

    /// <inheritdoc/>
    ICollection IDictionary.Keys => ((IDictionary)WrappedDictionary).Keys;

    /// <inheritdoc/>
    ICollection IDictionary.Values => ((IDictionary)WrappedDictionary).Values;

    /// <inheritdoc cref="IDictionary"/>
    bool ICollection.IsSynchronized => ((IDictionary)WrappedDictionary).IsSynchronized;

    /// <inheritdoc cref="IDictionary"/>
    object ICollection.SyncRoot => ((IDictionary)WrappedDictionary).SyncRoot;

    /// <inheritdoc cref="IDictionary"/>
    object IDictionary.this[object key]
    {
        get => ((IDictionary)WrappedDictionary)[key];
        set => ((IDictionary)WrappedDictionary)[key] = value;
    }

    #endregion
}
