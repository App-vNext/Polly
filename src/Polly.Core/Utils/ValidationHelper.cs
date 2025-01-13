using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Polly.Utils;

[ExcludeFromCodeCoverage]
internal static class ValidationHelper
{
    // Workaround for https://github.com/dotnet/runtime/issues/110917
    private static readonly object ValidatorLock = new();

    public static string[]? GetMemberName(this ValidationContext? validationContext) =>
#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
        validationContext?.MemberName is { } memberName
            ? new[] { memberName }
            : null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null

    public static string GetDisplayName(this ValidationContext? validationContext)
        => validationContext?.DisplayName ?? string.Empty;

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TimeSpan))]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "The member of options are preserved and not trimmed. See builder.AddStrategy() extension.")]
    public static void ValidateObject(ResilienceValidationContext context)
    {
        Guard.NotNull(context);

        bool valid;
        var errors = new List<ValidationResult>();

        lock (ValidatorLock)
        {
            valid = Validator.TryValidateObject(context.Instance, new(context.Instance), errors, true);
        }

        if (!valid)
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
