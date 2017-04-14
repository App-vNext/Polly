using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can
    /// be applied to delegates
    /// </summary>
    public abstract partial class Policy
    {
        private readonly Action<Action<CancellationToken>, Context, CancellationToken> _exceptionPolicy;
        private readonly IEnumerable<ExceptionPredicate> _exceptionPredicates;

        internal Policy(
            Action<Action<CancellationToken>, Context, CancellationToken> exceptionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException(nameof(exceptionPolicy));

            _exceptionPolicy = exceptionPolicy;
            _exceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            Execute(ct => action(), new Context(), CancellationToken.None);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        [DebuggerStepThrough]
        public void Execute(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            Execute(action, new Context(), cancellationToken);
        }
        
        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action action)
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
        public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture(action, new Context(), cancellationToken);
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
            return Execute(ct => action(), new Context(), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return Execute(action, new Context(), cancellationToken);
        }
        
        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action)
        {
            return ExecuteAndCapture(ct => action(), new Context(), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture(action, new Context(), cancellationToken);
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