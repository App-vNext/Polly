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
    public abstract partial class Policy : PolicyBase
    {
        private readonly Action<Action<Context, CancellationToken>, Context, CancellationToken> _exceptionPolicy;

        /// <summary>
        /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="exceptionPolicy"/> and <paramref name="exceptionPredicates"/> 
        /// </summary>
        /// <param name="exceptionPolicy">The execution policy that will be applied to delegates executed synchronously through the policy.</param>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        protected Policy(
            Action<Action<Context, CancellationToken>, Context, CancellationToken> exceptionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            _exceptionPolicy = exceptionPolicy ?? throw new ArgumentNullException(nameof(exceptionPolicy));
            ExceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [DebuggerStepThrough]
        internal virtual void ExecuteInternal(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");

            _exceptionPolicy(action, context, cancellationToken);
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
        internal virtual TResult ExecuteInternal<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_exceptionPolicy == null) throw new InvalidOperationException("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");

            var result = default(TResult);
            _exceptionPolicy((ctx, ct) => { result = action(ctx, ct); }, context, cancellationToken);
            return result;
        }

    }
}