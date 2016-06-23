using System;

namespace Polly
{
    /// <summary>
    /// Fluent API for chaining exceptions that will be handled by a <see cref="Policy"/>. 
    /// </summary>
    public static class OrSyntax
    {
        #region Add exception predicates to exception-filtering policy

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
        /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
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

        #endregion

        #region Add result predicates to exception-filtering policy

        /// <summary>
        /// Specifies the type of result that this policy can handle with additional filters on the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the results this policy will handle.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> OrResult<TResult>(this PolicyBuilder policyBuilder,
            Func<TResult, bool> resultPredicate)
        {
            return new PolicyBuilder<TResult>(policyBuilder.ExceptionPredicates).OrResult(resultPredicate);
        }

        #endregion

        #region Add result predicates to result-filtering policy

        /// <summary>
        /// Specifies the type of result that this policy can handle with additional filters on the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the results this policy will handle.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> OrResult<TResult>(this PolicyBuilder<TResult> policyBuilder,
            Func<TResult, bool> resultPredicate)
        {
            ResultPredicate<TResult> predicate = result => resultPredicate(result);
            policyBuilder.ResultPredicates.Add(predicate);
            return policyBuilder;
        }

        /// <summary>
        /// Specifies a result value which the policy will handle.
        /// </summary>
        /// <typeparam name="TResult">The type of the results this policy will handle.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <param name="result">The TResult value this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> OrResult<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult result)
        {
            return policyBuilder.OrResult(r => (r != null && r.Equals(result)) || (r == null && result == null));
        }

        #endregion

        #region Add exception predicates to result-filtering policy

        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <typeparam name="TResult">The type of the results this policy will handle.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> Or<TException, TResult>(this PolicyBuilder<TResult> policyBuilder) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;
            policyBuilder.ExceptionPredicates.Add(predicate);
            return policyBuilder;
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <typeparam name="TResult">The type of the results this policy will handle.</typeparam>
        /// <param name="policyBuilder">The current builder to chain off.</param>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> Or<TException, TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException)exception);

            policyBuilder.ExceptionPredicates.Add(predicate);
            return policyBuilder;
        }

        #endregion
    }

}