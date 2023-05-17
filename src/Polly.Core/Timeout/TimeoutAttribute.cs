using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Polly.Timeout;

internal sealed class TimeoutAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is TimeSpan timeSpan && !TimeoutUtil.IsTimeoutValid(timeSpan))
        {
            return new ValidationResult(string.Format(CultureInfo.InvariantCulture, TimeoutUtil.TimeSpanInvalidMessage, timeSpan));
        }

        return ValidationResult.Success!;
    }
}
