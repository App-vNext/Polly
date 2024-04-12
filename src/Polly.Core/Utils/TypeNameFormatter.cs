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

#if NET6_0_OR_GREATER
        var nameNoAirity = type.Name[..(type.Name.Length - GenericSuffixLength)];
#else
        var nameNoAirity = type.Name.Substring(0, type.Name.Length - GenericSuffixLength);
#endif

        return $"{nameNoAirity}<{Format(args[0])}>";
    }
}
