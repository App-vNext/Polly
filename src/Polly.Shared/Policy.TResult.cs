using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Polly.Utilities;

namespace Polly
{

    /// <summary>
    /// Transient fault handling policies that can be applied to delegates returning results of type <typeparamref name="TResult"/>
    /// </summary>
    public abstract partial class Policy<TResult>
    {
        private readonly Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> _executionPolicy;
        private readonly IEnumerable<ExceptionPredicate> _exceptionPredicates;
        private readonly IEnumerable<ResultPredicate<TResult>> _resultPredicates;

        internal Policy(
            Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            )
        {
            if (executionPolicy == null) throw new ArgumentNullException("executionPolicy");

            _executionPolicy = executionPolicy;
            _exceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
            _resultPredicates = resultPredicates ?? PredicateHelper<TResult>.EmptyResultPredicates;
        }

        #region Execute overloads

        /// <summary>
        /// Executes the specified action within the policy and returns the Result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<TResult> action)
        {
            return Execute(ct => action(), new Context(), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute(Func<TResult> action, IDictionary<string, object> contextData)
        {
            return Execute(ct => action(), new Context(contextData), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute(Func<TResult> action, Context context)
        {
            return Execute(ct => action(), context, CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return Execute(action, new Context(), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute(Func<CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return Execute(action, new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_executionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context);

            return _executionPolicy(action, context, cancellationToken);
        }

        #endregion

        #region ExecuteAndCapture overloads

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action)
        {
            return ExecuteAndCapture(ct => action(), new Context(), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCapture(ct => action(), new Context(contextData), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action, Context context)
        {
            return ExecuteAndCapture(ct => action(), context, CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture(action, new Context(), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture(action, new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_executionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                TResult result = Execute(action, context, cancellationToken);

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

        /// <summary>
        /// Gets the exception type
        /// </summary>
        /// <param name="exceptionPredicates"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        internal static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            return Policy.GetExceptionType(exceptionPredicates, exception);
        }

        #endregion
    }

}
