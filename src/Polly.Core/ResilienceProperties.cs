using System.Diagnostics.CodeAnalysis;

namespace Polly;

#pragma warning disable CA1710 // Identifiers should have correct suffix

/// <summary>
/// Represents a collection of custom resilience properties.
/// </summary>
public sealed class ResilienceProperties : IDictionary<string, object?>
{
    private Dictionary<string, object?> Options { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the value of a given property.
    /// </summary>
    /// <param name="key">Strongly typed key to get the value of property.</param>
    /// <param name="value">Returns the value of the property.</param>
    /// <typeparam name="TValue">The type of property value as defined by <paramref name="key"/> parameter.</typeparam>
    /// <returns>True, if an property is retrieved.</returns>
    public bool TryGetValue<TValue>(ResiliencePropertyKey<TValue> key, [MaybeNullWhen(false)] out TValue value)
    {
        if (Options.TryGetValue(key.Key, out object? val) && val is TValue typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets the value of a given property with a fallback default value.
    /// </summary>
    /// <param name="key">Strongly typed key to get the value of property.</param>
    /// <param name="defaultValue">The default value to use if property is not found.</param>
    /// <typeparam name="TValue">The type of property value as defined by <paramref name="key"/> parameter.</typeparam>
    /// <returns>The property value or the default value.</returns>
    public TValue GetValue<TValue>(ResiliencePropertyKey<TValue> key, TValue defaultValue)
    {
        if (TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets the value of a given property.
    /// </summary>
    /// <param name="key">Strongly typed key to get the value of property.</param>
    /// <param name="value">Returns the value of the property.</param>
    /// <typeparam name="TValue">The type of property value as defined by <paramref name="key"/> parameter.</typeparam>
    public void Set<TValue>(ResiliencePropertyKey<TValue> key, TValue value)
    {
        Options[key.Key] = value;
    }

    /// <inheritdoc/>
    object? IDictionary<string, object?>.this[string key]
    {
        get => Options[key];
        set => Options[key] = value;
    }

    /// <inheritdoc/>
    ICollection<string> IDictionary<string, object?>.Keys => Options.Keys;

    /// <inheritdoc/>
    ICollection<object?> IDictionary<string, object?>.Values => Options.Values;

    /// <inheritdoc/>
    int ICollection<KeyValuePair<string, object?>>.Count => Options.Count;

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => ((IDictionary<string, object?>)Options).IsReadOnly;

    /// <inheritdoc/>
    void IDictionary<string, object?>.Add(string key, object? value) => Options.Add(key, value);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => ((IDictionary<string, object?>)Options).Add(item);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, object?>>.Clear() => Options.Clear();

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) => ((IDictionary<string, object?>)Options).Contains(item);

    /// <inheritdoc/>
    bool IDictionary<string, object?>.ContainsKey(string key) => Options.ContainsKey(key);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
        ((IDictionary<string, object?>)Options).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => Options.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Options).GetEnumerator();

    /// <inheritdoc/>
    bool IDictionary<string, object?>.Remove(string key) => Options.Remove(key);

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => ((IDictionary<string, object?>)Options).Remove(item);

    /// <inheritdoc/>
    bool IDictionary<string, object?>.TryGetValue(string key, out object? value) => Options.TryGetValue(key, out value);
}

