using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Polly
{
    public abstract partial class PolicyV8<TResult> : ISyncPolicy<TResult>
    {
        #region Execute overloads

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<TResult> func)
        {
            return ((ISyncPolicyInternal<TResult>)this).Execute(
                new SyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<Context, TResult> func, IDictionary<string, object> contextData)
        {
            return ((ISyncPolicyInternal<TResult>)this).Execute(
                new SyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<Context, TResult> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).Execute(
                new SyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken);
        }
        
        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<CancellationToken, TResult> func, CancellationToken cancellationToken)
        {
            return ((ISyncPolicyInternal<TResult>)this).Execute(
                new SyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute(Func<Context, CancellationToken, TResult> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ((ISyncPolicyInternal<TResult>)this).Execute(
                new SyncExecutableFunc<TResult>(func),
                new Context(contextData),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute(Func<Context, CancellationToken, TResult> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).Execute(
                new SyncExecutableFunc<TResult>(func),
                context,
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>, and returns the result.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<T1>(Func<Context, CancellationToken, T1, TResult> func, Context context, CancellationToken cancellationToken, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).Execute(new SyncExecutableFunc<T1, TResult>(func, input1), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>, and returns the result.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<T1, T2>(Func<Context, CancellationToken, T1, T2, TResult> func, Context context, CancellationToken cancellationToken, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).Execute(new SyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken);
        }

        #endregion

        #region ExecuteAndCapture overloads

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> func)
        {
            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(
                new SyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> func, IDictionary<string, object> contextData)
        {
            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(
                new SyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(
                new SyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> func, CancellationToken cancellationToken)
        {
            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(
                new SyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(
                new SyncExecutableFunc<TResult>(func),
                new Context(contextData),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(
                new SyncExecutableFunc<TResult>(func),
                context,
                cancellationToken);
        }
        
        /// <summary>
        /// Executes the specified synchronous function within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>, and returns the captured result.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<T1>(Func<Context, CancellationToken, T1, TResult> func, Context context, CancellationToken cancellationToken, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(new SyncExecutableFunc<T1, TResult>(func, input1), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>, and returns the result.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<T1, T2>(Func<Context, CancellationToken, T1, T2, TResult> func, Context context, CancellationToken cancellationToken, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ((ISyncPolicyInternal<TResult>)this).ExecuteAndCapture(new SyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken);
        }

        #endregion
    }
}
