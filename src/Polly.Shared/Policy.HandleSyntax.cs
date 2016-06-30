using System;

namespace Polly
{
    public partial class Policy
    {

        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Handle<TException>() where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;

            return new PolicyBuilder(predicate);
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Handle<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException)exception);

            return new PolicyBuilder(predicate);
        }

        /// <summary>
        /// Specifies the type of return result that this policy can handle with additional filters on the result.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="resultPredicate">The predicate to filter results this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> HandleResult<TResult>(Func<TResult, bool> resultPredicate)
        {
            return new PolicyBuilder<TResult>(resultPredicate);
        }

        /// <summary>
        /// Specifies the type of return result that this policy can handle, and a result value which the policy will handle.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="result">The TResult value this policy will handle.</param>
        /// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> HandleResult<TResult>(TResult result) 
        {
            return HandleResult(new Func<TResult, bool>(r => (r != null && r.Equals(result)) || (r == null && result == null)));
        }
    }

    public partial class Policy<TResult>
    {
        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> Handle<TException>() where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;

            return new PolicyBuilder<TResult>(predicate);
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> Handle<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException)exception);

            return new PolicyBuilder<TResult>(predicate);
        }

        /// <summary>
        /// Specifies a filter on the return result values that this strongly-typed generic policy will handle.
        /// </summary>
        /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> HandleResult(Func<TResult, bool> resultPredicate)
        {
            return new PolicyBuilder<TResult>(resultPredicate);
        }

        /// <summary>
        /// Specifies a return result value which the strongly-typed generic policy will handle.
        /// </summary>
        /// <param name="result">The TResult value this policy will handle.</param>
        /// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder<TResult> HandleResult(TResult result)
        {
            return HandleResult(r => (r != null && r.Equals(result)) || (r == null && result == null));
        }
    }
}
