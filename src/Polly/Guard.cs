using System.Runtime.CompilerServices;

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

#if(NETFRAMEWORK || NETSTANDARD)
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter)]
    sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName) =>
            ParameterName = parameterName;

        public string ParameterName { get; }
    }
}
#endif