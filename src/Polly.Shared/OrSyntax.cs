using System;

namespace Polly
{
    /// <summary>
    /// Fluent API for chaining exceptions that will be handled by a <see cref="Policy"/>. 
    /// </summary>
    public static class OrSyntax
    {
        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Or<TException>(this PolicyBuilder policyBuilder) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;
            policyBuilder.ExceptionPredicates.Add(predicate);
            return policyBuilder;
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with addition filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Or<TException>(this PolicyBuilder policyBuilder, Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException)exception);

            policyBuilder.ExceptionPredicates.Add(predicate);
            return policyBuilder;
        }
    }
}