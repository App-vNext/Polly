using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly
{
    public abstract partial class Policy<TResult> 
    {
        private readonly Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> _asyncExecutionPolicy;

        /// <summary>
        /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="asyncExecutionPolicy"/>, <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/> 
        /// </summary>
        /// <param name="asyncExecutionPolicy">The execution policy that will be applied to delegates executed asychronously through the asynchronous policy.</param>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        /// <param name="resultPredicates">Predicates indicating which results the policy should handle. </param>
        protected Policy(
            Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates)
        {
            _asyncExecutionPolicy = asyncExecutionPolicy ?? throw new ArgumentNullException(nameof(asyncExecutionPolicy));
            ExceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
            ResultPredicates = resultPredicates ?? PredicateHelper<TResult>.EmptyResultPredicates;
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        internal Task<TResult> ExecuteAsyncInternal(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExecutionPolicy == null) throw new InvalidOperationException("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");

            return _asyncExecutionPolicy(
                    action,
                    context,
                    cancellationToken,
                    continueOnCapturedContext);
        }

    }

}
