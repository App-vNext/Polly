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
    public abstract partial class Policy<TResult> : PolicyBase
    {
        private readonly Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> _executionPolicy;
        internal IEnumerable<ResultPredicate<TResult>> ResultPredicates { get; }

        /// <summary>
        /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="executionPolicy"/>, <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/> 
        /// </summary>
        /// <param name="executionPolicy">The execution policy that will be applied to delegates executed synchronously through the policy.</param>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        /// <param name="resultPredicates">Predicates indicating which results the policy should handle. </param>
        protected Policy(
            Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            )
        {
            _executionPolicy = executionPolicy ?? throw new ArgumentNullException(nameof(executionPolicy));
            ExceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
            ResultPredicates = resultPredicates ?? PredicateHelper<TResult>.EmptyResultPredicates;
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        internal TResult ExecuteInternal(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_executionPolicy == null) throw new InvalidOperationException("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");

            return _executionPolicy(action, context, cancellationToken);
        }
    }

}
