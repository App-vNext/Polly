// Copyright (c) Microsoft Corporation. All Rights Reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

#pragma warning disable CA1019 // Define accessors for attribute arguments

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
internal sealed class TimeSpanAttribute : ValidationAttribute
{
    public TimeSpan Minimum => TimeSpan.Parse(_min, CultureInfo.InvariantCulture);

    public TimeSpan? Maximum => _max == null ? null : TimeSpan.Parse(_max, CultureInfo.InvariantCulture);

    private readonly string _min;
    private readonly string? _max;

    public TimeSpanAttribute(string min)
    {
        _min = min;
        _max = null;
    }

    public TimeSpanAttribute(string min, string max)
    {
        _min = min;
        _max = max;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext? validationContext)
    {
        var min = Minimum;
        var max = Maximum;

        if (value is TimeSpan ts)
        {
            if (ts < min)
            {
                return new ValidationResult($"The field {validationContext.GetDisplayName()} must be >= to {min}.", validationContext.GetMemberName());
            }

            if (max.HasValue)
            {
                if (ts > max.Value)
                {
                    return new ValidationResult($"The field {validationContext.GetDisplayName()} must be <= to {max}.", validationContext.GetMemberName());
                }
            }
        }

        return ValidationResult.Success!;
    }
}
