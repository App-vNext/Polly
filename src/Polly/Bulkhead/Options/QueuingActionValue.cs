using System;

namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="QueuingActionValue"/> is a range-safe type which also exposes a default value for the
/// <see cref="BulkheadPolicyOptionsBase.MaxQueuingActions"/> option.
/// </summary>
public class QueuingActionValue
{
    /// <summary>
    /// The default <see cref="QueuingActionValue"/>.
    /// </summary>
    public static QueuingActionValue Default { get; } = new(0);
    
    /// <summary>
    /// Converts a <see cref="QueuingActionValue"/> to an integer.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The value encoded by the object</returns>
    public static implicit operator int(QueuingActionValue value) => value._value;
    
    /// <summary>
    /// Converts an integer to a <see cref="QueuingActionValue"/>.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The object which encodes the value</returns>
    /// <exception cref="ArgumentOutOfRangeException">value;Value must be greater than or equal to zero.</exception>
    public static implicit operator QueuingActionValue(int value) => new(value);

    private readonly int _value;

    private QueuingActionValue(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero.");
        _value = value;
    }
}