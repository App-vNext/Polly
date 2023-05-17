using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Polly.Utils;

[ExcludeFromCodeCoverage]
internal static class ValidationHelper
{
    public static void ValidateObject(object instance, string mainMessage)
    {
        var errors = new List<ValidationResult>();

        if (!Validator.TryValidateObject(instance, new ValidationContext(instance), errors, true))
        {
            var stringBuilder = new StringBuilder(mainMessage);
            stringBuilder.AppendLine();

            stringBuilder.AppendLine("Validation Errors:");
            foreach (var error in errors)
            {
                stringBuilder.AppendLine(error.ErrorMessage);
            }

            throw new ValidationException(stringBuilder.ToString());
        }
    }
}
