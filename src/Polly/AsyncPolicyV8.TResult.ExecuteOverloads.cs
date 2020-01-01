﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    public abstract partial class AsyncPolicyV8<TResult> : IAsyncPolicy<TResult>
    {
        private async Task<TResult> DespatchExecutionAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return await ImplementationAsyncV8(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        private async Task<PolicyResult<TResult>> DespatchExecuteAndCaptureAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                TResult result = await DespatchExecutionAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);

                if (ResultPredicates.AnyMatch(result))
                {
                    return PolicyResult<TResult>.Failure(result, context);
                }

                return PolicyResult<TResult>.Successful(result, context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        #region ExecuteAsync overloads

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Task<TResult>> func)
        {
            return DespatchExecutionAsync(
                new AsyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> func, IDictionary<string, object> contextData)
        {
            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the function</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecutionAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                continueOnCapturedContext);
        }

        #endregion

        #region ExecuteAndCaptureAsync overloads

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Task<TResult>> func)
        {
            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> func, IDictionary<string, object> contextData)
        {
            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                continueOnCapturedContext);
        }

        #endregion
    }
}
