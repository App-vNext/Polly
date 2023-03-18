namespace Polly;

#pragma warning disable CA1815 // Override equals and operator equals on value types, this type should never be used as key to dictionary

/// <summary>
/// Represents a key used by <see cref="ResilienceProperties"/>.
/// </summary>
/// <typeparam name="TValue">The type of the value of the property.</typeparam>
public readonly struct ResiliencePropertyKey<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResiliencePropertyKey{TValue}"/> struct.
    /// </summary>
    /// <param name="key">The key name.</param>
    public ResiliencePropertyKey(string key)
    {
        Guard.NotNull(key);

        Key = key;
    }

    /// <summary>
    /// Gets the name of the key.
    /// </summary>
    public string Key { get; }

    /// <inheritdoc/>
    public override string ToString() => Key;
}

