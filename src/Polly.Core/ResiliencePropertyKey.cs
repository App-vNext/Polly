namespace Polly;

/// <summary>
/// Represents a key used by <see cref="ResilienceProperties"/>.
/// </summary>
/// <typeparam name="TValue">The type of the value of the property.</typeparam>
public readonly struct ResiliencePropertyKey<TValue> : IEquatable<ResiliencePropertyKey<TValue>>
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

    /// /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ResiliencePropertyKey<TValue> other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ResiliencePropertyKey<TValue> other) => StringComparer.Ordinal.Equals(Key, other.Key);

    /// <inheritdoc/>
    public override int GetHashCode() => (StringComparer.Ordinal.GetHashCode(Key), typeof(TValue)).GetHashCode();

    /// <summary>
    /// The operator to compare two instances of <see cref="ResiliencePropertyKey{TValue}"/> for equality.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns>True if the instances are equal, false otherwise.</returns>
    public static bool operator ==(ResiliencePropertyKey<TValue> left, ResiliencePropertyKey<TValue> right) => left.Equals(right);

    /// <summary>
    /// The operator to compare two instances of <see cref="ResiliencePropertyKey{TValue}"/> for inequality.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns>True if the instances are not equal, false otherwise.</returns>
    public static bool operator !=(ResiliencePropertyKey<TValue> left, ResiliencePropertyKey<TValue> right) => !(left == right);
}

