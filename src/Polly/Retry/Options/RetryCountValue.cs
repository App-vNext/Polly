#nullable enable
using System;

namespace Polly.Retry.Options;

/// <summary>
/// The <see cref="RetryCountValue"/> is a range-safe type which also exposes a default value for the
/// <see cref="RetryPolicyOptionsBase.PermittedRetryCount"/> option.
/// </summary>
public class RetryCountValue
{
    /// <summary>
    /// The default <see cref="RetryCountValue"/>.
    /// </summary>
    public static RetryCountValue Infinity { get; } = new(-1);
    
    /// <summary>
    /// Converts a <see cref="RetryCountValue"/> to an integer.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The value encoded by the object</returns>
    public static implicit operator int(RetryCountValue value) => value._value;
    
    /// <summary>
    /// Converts an integer to a <see cref="RetryCountValue"/>.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The object which encodes the value</returns>
    /// <exception cref="ArgumentOutOfRangeException">value;Value must be greater than or equal to -1.</exception>
    public static implicit operator RetryCountValue(int value) => new(value);

    private readonly int _value;

    private RetryCountValue(int value)
    {
        if (value < -1)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero.");
        _value = value;
    }
}