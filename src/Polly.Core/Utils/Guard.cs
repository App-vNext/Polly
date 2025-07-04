using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Polly.Utils;

[ExcludeFromCodeCoverage]
internal static class Guard
{
    public static T NotNull<T>(T value, [CallerArgumentExpression("value")] string argumentName = "")
        where T : class
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value, argumentName);
#else
        if (value is null)
        {
            throw new ArgumentNullException(argumentName);
        }
#endif

        return value;
    }
}
