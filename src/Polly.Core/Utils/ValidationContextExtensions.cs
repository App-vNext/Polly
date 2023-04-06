using System.ComponentModel.DataAnnotations;

internal static class ValidationContextExtensions
{
    public static string[]? GetMemberName(this ValidationContext? validationContext)
    {
#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
        return validationContext?.MemberName is { } memberName
            ? new[] { memberName }
            : null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null
    }

    public static string GetDisplayName(this ValidationContext? validationContext)
    {
        return validationContext?.DisplayName ?? string.Empty;
    }
}
