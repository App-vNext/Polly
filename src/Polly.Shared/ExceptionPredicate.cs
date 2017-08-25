using System;

namespace Polly
{
    /// <summary>
    /// Predicate for validating the exception type and optional exception predicate
    /// </summary>
    /// <param name="ex">The exception to validate</param>
    /// <returns>True if valid</returns>
    public delegate bool ExceptionPredicate(Exception ex);
}