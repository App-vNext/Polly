using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly
{
    public abstract partial class Policy : IAsyncPolicy
    {
        private readonly Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> _asyncExceptionPolicy;

        /// <summary>
        /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="asyncExceptionPolicy"/> and <paramref name="exceptionPredicates"/>.
        /// </summary>
        /// <param name="asyncExceptionPolicy">The execution policy that will be applied to delegates executed asychronously through the asynchronous policy.</param>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        protected Policy(
            Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            _asyncExceptionPolicy = asyncExceptionPolicy ?? throw new ArgumentNullException(nameof(asyncExceptionPolicy));
            ExceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        internal async Task ExecuteAsyncInternal(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");

            await _asyncExceptionPolicy(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        internal virtual async Task<TResult> ExecuteAsyncInternal<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException(
                "Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");

            var result = default(TResult);
            await _asyncExceptionPolicy(async (ctx, ct) =>
            {
                result = await action(ctx, ct).ConfigureAwait(continueOnCapturedContext);
            }, context, cancellationToken, continueOnCapturedContext)
                .ConfigureAwait(continueOnCapturedContext);
            return result;
        }

    }
}
