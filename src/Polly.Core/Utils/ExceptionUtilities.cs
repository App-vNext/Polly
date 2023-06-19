namespace Polly.Utils;

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

internal static class ExceptionUtilities
{
#if !NET6_0_OR_GREATER
    private static readonly FieldInfo StackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly Type TraceFormat = Type.GetType("System.Diagnostics.StackTrace")!.GetNestedType("TraceFormat", BindingFlags.NonPublic)!;
    private static readonly MethodInfo TraceToString = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { TraceFormat }, null)!;
#endif

    public static T TrySetStackTrace<T>(this T exception)
        where T : Exception
    {
        if (!string.IsNullOrWhiteSpace(exception.StackTrace))
        {
            return exception;
        }

#if NET6_0_OR_GREATER
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.SetCurrentStackTrace(exception);
#else
        SetStackTrace(exception, new StackTrace());
#endif
        return exception;
    }

#if !NET6_0_OR_GREATER
    private static void SetStackTrace(this Exception target, StackTrace stack)
    {
        var getStackTraceString = TraceToString.Invoke(stack, new[] { Enum.GetValues(TraceFormat).GetValue(0) });
        StackTraceString.SetValue(target, getStackTraceString);
    }
#endif
}
