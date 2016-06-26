using System;
using System.Collections.Generic;
using System.Text;

namespace Polly
{
    public partial class PolicyBuilder
    {
        #region Add exception predicates to exception-filtering policy

        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder Or<TException>() where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;
            ExceptionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder Or<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException) exception);

            ExceptionPredicates.Add(predicate);
            return this;
        }

        #endregion

        #region Add result predicates to exception-filtering policy

        /// <summary>
        /// Specifies the type of result that this policy can handle with additional filters on the result.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder<TResult> OrResult<TResult>(Func<TResult, bool> resultPredicate)
        {
            return new PolicyBuilder<TResult>(ExceptionPredicates).OrResult(resultPredicate);
        }

        /// <summary>
        /// Specifies a result value which the policy will handle.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="result">The TResult value this policy will handle.</param>
        /// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder<TResult> OrResult<TResult>(TResult result)
        {
            return OrResult<TResult>(r => (r != null && r.Equals(result)) || (r == null && result == null));
        }

        #endregion
    }
    public partial class PolicyBuilder<TResult>
    {

        #region Add result predicates to result-filtering policy

        /// <summary>
        /// Specifies the type of result that this policy can handle with additional filters on the result.
        /// </summary>
        /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder<TResult> OrResult(Func<TResult, bool> resultPredicate)
        {
            ResultPredicate<TResult> predicate = result => resultPredicate(result);
            ResultPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Specifies a result value which the policy will handle.
        /// </summary>
        /// <param name="result">The TResult value this policy will handle.</param>
        /// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder<TResult> OrResult(TResult result)
        {
            return OrResult(r => (r != null && r.Equals(result)) || (r == null && result == null));
        }

        #endregion

        #region Add exception predicates to result-filtering policy

        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder<TResult> Or<TException>() where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;
            ExceptionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyBuilder<TResult> Or<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException)exception);

            ExceptionPredicates.Add(predicate);
            return this;
        }

        #endregion
    }
}
