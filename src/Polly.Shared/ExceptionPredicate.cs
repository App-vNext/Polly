using System;

namespace Polly
{
    /// <summary>
    /// A predicate that can be run against a passed <see cref="Exception"/> <paramref name="ex"/>.  
    /// </summary>
    /// <param name="ex">The passed exception, against which to evaluate the predicate.</param>
    /// <returns>A matched <see cref="Exception"/>; or null, if an exception was not matched.  ExceptionPredicate implementations may return the passed Exception <paramref name="ex"/>, indicating that it matched the predicate. They may also return inner exceptions of the passed Exception <paramref name="ex"/>, to indicate that the returned inner exception matched the predicate.</returns>
    public delegate Exception ExceptionPredicate(Exception ex); 
}