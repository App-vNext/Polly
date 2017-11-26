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

        internal Policy(
            Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            _asyncExceptionPolicy = asyncExceptionPolicy ?? throw new ArgumentNullException(nameof(asyncExceptionPolicy));
            ExceptionPredicates = exceptionPredicates ?? PredicateHelper.EmptyExceptionPredicates;
        }


        #region ExecuteAsync overloads

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action, IDictionary<string, object> contextData)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action, Context context)
        {
            return ExecuteAsync((ctx, ct) => action(), context, CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, Context context)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), context, CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAsync(action, new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAsync(action, context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, continueOnCapturedContext);
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
        public Task ExecuteAsync(Func<CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ct), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);
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
        public async Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context);

            await _asyncExceptionPolicy(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        #region Overloads method-generic in TResult

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, IDictionary<string, object> contextData)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, Context context)
        {
            return ExecuteAsync((ctx, ct) => action(), context, CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, Context context)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), context, CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAsync(action, new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAsync(action, context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, continueOnCapturedContext);
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
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync((ctx, ct) => action(ct), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);
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
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            SetPolicyContext(context);

            return ExecuteAsyncInternal(action, context, cancellationToken, continueOnCapturedContext);
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
        public virtual async Task<TResult> ExecuteAsyncInternal<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
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

        #endregion

        #endregion

        #region ExecuteAndCaptureAsync overloads

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action, Context context)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), context, CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), context, CancellationToken.None, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(action, context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public async Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                await ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
                return PolicyResult.Successful(context);
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        #region Overloads method-generic in TResult

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(), CancellationToken.None, false);
        }


        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, Context context)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), context, CancellationToken.None, false);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, false);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, Context context)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), context, CancellationToken.None, false);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, Context context, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), context, CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(action, context, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(contextData), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
        [DebuggerStepThrough]
        public async Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException(
                "Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(
                    await ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext), context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        #endregion

        #endregion
    }
}
