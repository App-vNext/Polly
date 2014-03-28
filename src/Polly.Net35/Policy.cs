using System;
using System.Diagnostics;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can
    /// be applied to delegates
    /// </summary>
    public class Policy
    {
        private readonly Action<Action> _exceptionPolicy;

        internal Policy(Action<Action> exceptionPolicy)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");
            
            _exceptionPolicy = exceptionPolicy;
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            _exceptionPolicy(action);
        }


        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action)
        {
            var result = default(TResult);
            _exceptionPolicy(() => { result = action(); });
            return result;
        }

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
        /// Specifies the type of exception that this policy can handle with addition filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Handle<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException) exception);

            return new PolicyBuilder(predicate);
        }

        /// <summary>
        /// Builds a void policy without behaviour.
        /// </summary>
        /// <returns>A policy instance</returns>
        public static Policy Empty()
        {
            return new Policy(x => x());
        }
    }
}