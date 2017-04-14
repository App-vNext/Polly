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

        #region Execute overloads

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
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public void Execute(Action action, IDictionary<string, object> contextData)
        {
            Execute(ct => action(), new Context(contextData), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public void Execute(Action action, Context context)
        {
            Execute(ct => action(), context, CancellationToken.None);
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
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public void Execute(Action<CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            Execute(action, new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [DebuggerStepThrough]
        public void Execute(Action<CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context);

            _exceptionPolicy(action, context, cancellationToken);
        }
        
        #region Overloads method-generic in TResult

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
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action, IDictionary<string, object> contextData)
        {
            return Execute(ct => action(), new Context(contextData), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action, Context context)
        {
            return Execute(ct => action(), context, CancellationToken.None);
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
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return Execute(action, new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context);

            var result = default(TResult);
            _exceptionPolicy(ct => { result = action(ct); }, context, cancellationToken);
            return result;
        }

        #endregion

        #endregion

        #region ExecuteAndCapture overloads

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
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCapture(ct => action(), new Context(contextData), CancellationToken.None);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action action, Context context)
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
        public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken)
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
        public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture(action, new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                Execute(action, context, cancellationToken);
                return PolicyResult.Successful();
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
        }

        #region Overloads method-generic in TResult

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
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action, IDictionary<string, object> contextData)
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
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action, Context context)
        {
            return ExecuteAndCapture(ct => action(), context, CancellationToken.None);
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


        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
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
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException(
                "Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(Execute(action, context, cancellationToken));
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
        }

        #endregion

        internal static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            var isExceptionTypeHandledByThisPolicy = exceptionPredicates.Any(predicate => predicate(exception));

            return isExceptionTypeHandledByThisPolicy
                ? ExceptionType.HandledByThisPolicy
                : ExceptionType.Unhandled;
        }

        #endregion
    }
}