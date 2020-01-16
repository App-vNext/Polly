using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    public abstract partial class AsyncPolicy : IAsyncPolicy
    {
        #region ExecuteAsync overloads

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionNoParams(action), GetDefaultExecutionContext(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnContext(action), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnContext(action), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        [DebuggerStepThrough]
        public Task ExecuteAsync<T1>(Func<Context, CancellationToken, bool, T1, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableAction<T1>(action, input1), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        [DebuggerStepThrough]
        public Task ExecuteAsync<T1, T2>(Func<Context, CancellationToken, bool, T1, T2, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync(new AsyncExecutableAction<T1, T2>(action, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #region Overloads method-generic in TResult

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncNoParams<TResult>, TResult>(
                new AsyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> func, IDictionary<string, object> contextData)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
                new AsyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
                new AsyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the function</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<T1, TResult>(Func<Context, CancellationToken, bool, T1, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFunc<T1, TResult>, TResult>(new AsyncExecutableFunc<T1,TResult>(func, input1), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<T1, T2, TResult>(Func<Context, CancellationToken, bool, T1, T2, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAsync<AsyncExecutableFunc<T1, T2, TResult>, TResult>(new AsyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #endregion

        #endregion

        #region ExecuteAndCaptureAsync overloads

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionNoParams(action), GetDefaultExecutionContext(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnContext(action), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnContext(action), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync<T1>(Func<Context, CancellationToken, bool, T1, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableAction<T1>(action, input1), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync<T1, T2>(Func<Context, CancellationToken, bool, T1, T2, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync(new AsyncExecutableAction<T1, T2>(action, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #region Overloads method-generic in TResult

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> func)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncNoParams<TResult>, TResult>(
                new AsyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> func, IDictionary<string, object> contextData)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
                new AsyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
                new AsyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                new Context(contextData),
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
                new AsyncExecutableFuncOnContextCancellationToken<TResult>(func),
                context,
                cancellationToken,
                continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>, and returns the captured result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<T1, TResult>(Func<Context, CancellationToken, bool, T1, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFunc<T1, TResult>, TResult>(new AsyncExecutableFunc<T1,TResult>(func, input1), context, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous function within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a promise of a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<T1, T2, TResult>(Func<Context, CancellationToken, bool, T1, T2, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((IAsyncPolicyInternal)this).ExecuteAndCaptureAsync<AsyncExecutableFunc<T1, T2, TResult>, TResult>(new AsyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #endregion

        #endregion

    }
}
