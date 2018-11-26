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
        #region ExecuteAsync overloads

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            return ExecuteAsync((ctx, ct) => action(), new Context(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, Context context)
        {
            return ExecuteAsync((ctx, ct) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ExecuteAsync((ctx, ct) => action(ct), new Context(), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);
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
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                await ExecuteAsyncInternal(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
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
            return ExecuteAsync((ctx, ct) => action(), new Context(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync((ctx, ct) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync((ctx, ct) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync((ctx, ct) => action(ct), new Context(), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);
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
        public async Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return await ExecuteAsyncInternal(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(), new Context(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync((ctx, ct) => action(ct), new Context(), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
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
            return ExecuteAndCaptureAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);
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
