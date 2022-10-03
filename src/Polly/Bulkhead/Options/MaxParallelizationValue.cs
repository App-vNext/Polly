using System;

namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="QueuingActionValue"/> is a range-safe type which also exposes a default value for the
/// <see cref="BulkheadPolicyOptionsBase.MaxParallelization"/> option.
/// </summary>
public class MaxParallelizationValue
{
    /// <summary>
    /// Converts a <see cref="MaxParallelizationValue"/> to an integer.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The value encoded by the object</returns>
    public static implicit operator int(MaxParallelizationValue value) => value._value;
    
    /// <summary>
    /// Converts an integer to a <see cref="MaxParallelizationValue"/>.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The object which encodes the value</returns>
    /// <exception cref="ArgumentOutOfRangeException">value;Value must be greater than zero.</exception>
    public static implicit operator MaxParallelizationValue(int value) => new(value);
    
    private readonly int _value;
    
    private MaxParallelizationValue(int value)
    {
        if (value <= 0) 
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than zero.");
        _value = value;
    }
}