using System;

namespace Polly.Utils;

internal static class TypeNameFormatter
{
    private const int GenericSuffixLength = 2;

    public static string Format(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var args = type.GetGenericArguments();
        if (args.Length != 1)
        {
            return type.Name;
        }

        return $"{type.Name.Substring(0, type.Name.Length - GenericSuffixLength)}<{Format(args[0])}>";
    }
}
