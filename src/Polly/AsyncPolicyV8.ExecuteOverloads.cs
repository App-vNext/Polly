using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly
{
    public abstract partial class AsyncPolicyV8 : IAsyncPolicy
    {
        private async Task<TResult> DespatchExecutionAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return await ImplementationAsyncV8<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        private async Task<PolicyResult> DespatchExecuteAndCaptureAsync(IAsyncExecutable<EmptyStruct> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                await DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
                return PolicyResult.Successful(context);
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        private async Task<PolicyResult<TResult>> DespatchExecuteAndCaptureAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(
                    await DespatchExecutionAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext), 
                    context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        #region ExecuteAsync overloads

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionNoParams(action), GetDefaultExecutionContext(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnContext(action), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
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

            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnContext(action), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, DefaultContinueOnCapturedContext);
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
            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
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

            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, DefaultContinueOnCapturedContext);
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
            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, continueOnCapturedContext);
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
            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, continueOnCapturedContext);
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

            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, continueOnCapturedContext);
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

            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableAction<T1>(action, input1), context, cancellationToken, continueOnCapturedContext);
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

            return DespatchExecutionAsync<IAsyncExecutable<EmptyStruct>, EmptyStruct>(new AsyncExecutableAction<T1, T2>(action, input1, input2), context, cancellationToken, continueOnCapturedContext);
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
            return DespatchExecutionAsync<AsyncExecutableFuncNoParams<TResult>, TResult>(
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
            return DespatchExecutionAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
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

            return DespatchExecutionAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
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
            return DespatchExecutionAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
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
            return DespatchExecutionAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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

            return DespatchExecutionAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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
            return DespatchExecutionAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
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
            return DespatchExecutionAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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

            return DespatchExecutionAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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

            return DespatchExecutionAsync<IAsyncExecutable<TResult>, TResult>(new AsyncExecutableFunc<T1,TResult>(func, input1), context, cancellationToken, continueOnCapturedContext);
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

            return DespatchExecutionAsync<IAsyncExecutable<TResult>, TResult>(new AsyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #endregion

        #endregion

        #region ExecuteAndCaptureAsync overloads

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action)
        {
            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionNoParams(action), GetDefaultExecutionContext(), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnContext(action), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnContext(action), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, DefaultContinueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
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
            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), new Context(contextData), cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(new AsyncExecutableActionOnContextCancellationToken(action), context, cancellationToken, continueOnCapturedContext);
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
        /// <returns>The value returned by the function, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync<T1>(Func<Context, CancellationToken, bool, T1, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(new AsyncExecutableAction<T1>(action, input1), context, cancellationToken, continueOnCapturedContext);
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
        /// <returns>The value returned by the function, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync<T1, T2>(Func<Context, CancellationToken, bool, T1, T2, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync(new AsyncExecutableAction<T1, T2>(action, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #region Overloads method-generic in TResult

        /// <summary>
        /// Executes the specified asynchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> func)
        {
            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncNoParams<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> func, IDictionary<string, object> contextData)
        {
            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnContext<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> func, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnCancellationToken<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync<AsyncExecutableFuncOnContextCancellationToken<TResult>, TResult>(
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
        /// <returns>The value returned by the function, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<T1, TResult>(Func<Context, CancellationToken, bool, T1, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync<IAsyncExecutable<TResult>, TResult>(new AsyncExecutableFunc<T1,TResult>(func, input1), context, cancellationToken, continueOnCapturedContext);
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
        /// <returns>The value returned by the function, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<T1, T2, TResult>(Func<Context, CancellationToken, bool, T1, T2, Task<TResult>> func, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCaptureAsync<IAsyncExecutable<TResult>, TResult>(new AsyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken, continueOnCapturedContext);
        }

        #endregion

        #endregion

    }
}
