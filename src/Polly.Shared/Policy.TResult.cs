using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return Execute(action, new Context(), cancellationToken);
        }

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
        /// Gets the exception type
        /// </summary>
        /// <param name="exceptionPredicates"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        internal static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            return Policy.GetExceptionType(exceptionPredicates, exception);
        }
    }

}
