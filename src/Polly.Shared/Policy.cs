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

        internal Policy(
            Action<Action> exceptionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates
            ) : this((action, ctx) => exceptionPolicy(action), exceptionPredicates)
        {
        }

        internal Policy(
            Action<Action, Context> exceptionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");

            _exceptionPolicy = exceptionPolicy;
            _exceptionPredicates = exceptionPredicates ?? Enumerable.Empty<ExceptionPredicate>();

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

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        protected PolicyResult ExecuteAndCapture(Action action, Context context)
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

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        protected PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action, Context context)
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

        internal static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            var isExceptionTypeHandledByThisPolicy = exceptionPredicates.Any(predicate => predicate(exception));

            return isExceptionTypeHandledByThisPolicy
                ? ExceptionType.HandledByThisPolicy
                : ExceptionType.Unhandled;
        }
    }

    /// <summary>
    /// Transient fault handling policies that can be applied to delegates returning results of type <typeparam name="TResult"/>
    /// </summary>
    public partial class Policy<TResult>
    {
        private readonly Func<Func<TResult>, Context, TResult> _executionPolicy;
        private readonly IEnumerable<ExceptionPredicate> _exceptionPredicates;
        private readonly IEnumerable<ResultPredicate<TResult>> _resultPredicates;

        internal Policy(
            Func<Func<TResult>, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates, 
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : this(
                  (action, ctx) => executionPolicy(action), 
                  exceptionPredicates, 
                  resultPredicates
                )
        {
        }

        internal Policy(
            Func<Func<TResult>, Context, TResult> executionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates, 
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            )
        {
            if (executionPolicy == null) throw new ArgumentNullException("executionPolicy");

            _executionPolicy = executionPolicy;
            _exceptionPredicates = exceptionPredicates ?? Enumerable.Empty<ExceptionPredicate>();
            _resultPredicates = resultPredicates ?? Enumerable.Empty<ResultPredicate<TResult>>();
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        protected TResult Execute(Func<TResult> action, Context context)
        {
            if (_executionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            return _executionPolicy(action, context);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the Result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<TResult> action)
        {
            return Execute(action, Context.Empty);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action)
        {
            return ExecuteAndCapture(action, Context.Empty);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        protected PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action, Context context)
        {

            if (_executionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous ExecuteAndCapture method.");

            try
            {
                TResult result = _executionPolicy(action, context);

                if (_resultPredicates.Any(predicate => predicate(result)))
                {
                    return PolicyResult<TResult>.Failure(result);
                }

                return PolicyResult<TResult>.Successful(result);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
        }

        internal static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            return Policy.GetExceptionType(exceptionPredicates, exception);
        }
    }
}