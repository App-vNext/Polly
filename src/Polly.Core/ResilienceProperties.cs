using System.Diagnostics.CodeAnalysis;

namespace Polly;

/// <summary>
/// Represents a collection of custom resilience properties.
/// </summary>
[DebuggerDisplay("{Options}")]
public sealed class ResilienceProperties
{
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal IDictionary<string, object?> Options { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the value of a given property.
    /// </summary>
    /// <param name="key">Strongly typed key to get the value of the property.</param>
    /// <param name="value">Returns the value of the property.</param>
    /// <typeparam name="TValue">The type of property value as defined by <paramref name="key"/> parameter.</typeparam>
    /// <returns>True, if a property was retrieved.</returns>
    public bool TryGetValue<TValue>(ResiliencePropertyKey<TValue> key, [MaybeNullWhen(false)] out TValue value)
    {
        if (Options.TryGetValue(key.Key, out object? val))
        {
            if (val is TValue typedValue)
            {
                value = typedValue;
                return true;
            }
            else if (val is null)
            {
                value = default!;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets the value of a given property with a fallback default value.
    /// </summary>
    /// <param name="key">Strongly typed key to get the value of the property.</param>
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
    /// <param name="key">Strongly typed key to get the value of the property.</param>
    /// <param name="value">Returns the value of the property.</param>
    /// <typeparam name="TValue">The type of property value as defined by <paramref name="key"/> parameter.</typeparam>
    public void Set<TValue>(ResiliencePropertyKey<TValue> key, TValue value) => Options[key.Key] = value;

    internal void AddOrReplaceProperties(ResilienceProperties other)
    {
        // try to avoid enumerator allocation
        if (other.Options is Dictionary<string, object?> otherOptions)
        {
            foreach (var pair in otherOptions)
            {
                Options[pair.Key] = pair.Value;
            }
        }
        else
        {
            foreach (var pair in other.Options)
            {
                Options[pair.Key] = pair.Value;
            }
        }
    }
}

