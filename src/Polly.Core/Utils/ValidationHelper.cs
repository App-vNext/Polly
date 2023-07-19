using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Polly.Utils;

[ExcludeFromCodeCoverage]
internal static class ValidationHelper
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TimeSpan))]
    [RequiresUnreferencedCode("Calls System.ComponentModel.DataAnnotations.ValidationContext.ValidationContext(Object)")]
    public static void ValidateObject(ResilienceValidationContext context)
    {
        Guard.NotNull(context);

        var errors = new List<ValidationResult>();

        if (!Validator.TryValidateObject(context.Instance, new ValidationContext(context.Instance), errors, true))
        {
            var stringBuilder = new StringBuilder(context.PrimaryMessage);
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
