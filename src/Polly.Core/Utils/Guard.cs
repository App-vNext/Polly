using System.Runtime.CompilerServices;

namespace Polly.Utils;

internal static class Guard
{
    public static T NotNull<T>(T value, [CallerArgumentExpression("value")] string argumentName = "")
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(argumentName);
        }

        return value;
    }
}
