#nullable enable
using System.Runtime.ExceptionServices;

namespace Polly.Utilities;

/// <summary>
/// Contains extension methods on the <see cref="Exception"/> class.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Rethrows the extended <see cref="Exception"/>, <paramref name="exceptionPossiblyToThrow"/>, using the <see cref="ExceptionDispatchInfo"/> class to rethrow it with its original stack trace, if <paramref name="exceptionPossiblyToThrow"/> differs from <paramref name="exceptionToCompare"/>.
    /// </summary>
    /// <param name="exceptionPossiblyToThrow">The exception to throw, if it differs from <paramref name="exceptionToCompare"/>.</param>
    /// <param name="exceptionToCompare">The exception to compare against.</param>
    public static void RethrowWithOriginalStackTraceIfDiffersFrom(this Exception exceptionPossiblyToThrow, Exception exceptionToCompare)
    {
        if (exceptionPossiblyToThrow != exceptionToCompare)
        {
            ExceptionDispatchInfo.Capture(exceptionPossiblyToThrow).Throw();
        }
    }
}
