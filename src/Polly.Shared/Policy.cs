using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can
    /// be applied to delegates
    /// </summary>
    public partial class Policy
    {
        private readonly Action<Action, Context> _exceptionPolicy;
        private readonly IEnumerable<ExceptionPredicate> _exceptionPredicates;

        internal Policy(Action<Action, Context> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");

            _exceptionPolicy = exceptionPolicy;
            _exceptionPredicates = exceptionPredicates ?? Enumerable.Empty<ExceptionPredicate>();

        }

        internal Policy(Action<Action> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
            : this((action, ctx) => exceptionPolicy(action), exceptionPredicates)
        {
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            Execute(action, Context.Empty);
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        protected void Execute(Action action, Context context)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            _exceptionPolicy(action, context);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action action)
        {
            return ExecuteAndCapture(action, Context.Empty);
        }

        [DebuggerStepThrough]
        private PolicyResult ExecuteAndCapture(Action action, Context context)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous ExecuteAndCapture method.");

            try
            {
                _exceptionPolicy(action, context);
                return PolicyResult.Successful();
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        protected TResult Execute<TResult>(Func<TResult> action, Context context)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            var result = default(TResult);
            _exceptionPolicy(() => { result = action(); }, context);
            return result;
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the Result.
        /// </summary>
        /// <typeparam name="TResult">The type of the Result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action)
        {
            return Execute(action, Context.Empty);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action)
        {
            return ExecuteAndCapture(action, Context.Empty);
        }

        [DebuggerStepThrough]
        private PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action, Context context)
        {

            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous ExecuteAndCapture method.");

            try
            {
                var result = default(TResult);
                _exceptionPolicy(() => { result = action(); }, context);
                return PolicyResult<TResult>.Successful(result);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
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
                                                        exceptionPredicate((TException)exception);

            return new PolicyBuilder(predicate);
        }

        internal static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            var isExceptionTypeHandledByThisPolicy = exceptionPredicates.Any(predicate => predicate(exception));

            return isExceptionTypeHandledByThisPolicy
                ? ExceptionType.HandledByThisPolicy
                : ExceptionType.Unhandled;
        }
    }
}