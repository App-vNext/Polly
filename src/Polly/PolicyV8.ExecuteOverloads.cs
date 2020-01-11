using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Polly.Utilities;

namespace Polly
{
    public abstract partial class PolicyV8 : ISyncPolicy
    {
        private void DespatchNonGenericExecution(in ISyncExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                SyncNonGenericImplementationV8(action, context, cancellationToken);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        private TResult DespatchGenericExecution<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return SyncGenericImplementationV8<TExecutable, TResult>(action, context, cancellationToken);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        private PolicyResult DespatchExecuteAndCapture(in ISyncExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                DespatchNonGenericExecution(action, context, cancellationToken);
                return PolicyResult.Successful(context);
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        private PolicyResult<TResult> DespatchExecuteAndCapture<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(
                    DespatchGenericExecution<TExecutable, TResult>(action, context, cancellationToken), 
                    context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        #region Execute overloads

        /// <summary>
        /// Executes the specified synchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            DespatchNonGenericExecution(new SyncExecutableActionNoParams(action), GetDefaultExecutionContext(), DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public void Execute(Action<Context> action, IDictionary<string, object> contextData)
        {
            DespatchNonGenericExecution(new SyncExecutableActionOnContext(action), new Context(contextData), DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        [DebuggerStepThrough]
        public void Execute(Action<Context> action, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DespatchNonGenericExecution(new SyncExecutableActionOnContext(action), context, DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public void Execute(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            DespatchNonGenericExecution(new SyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public void Execute(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            DespatchNonGenericExecution(new SyncExecutableAction(action), new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public void Execute(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DespatchNonGenericExecution(new SyncExecutableAction(action), context, cancellationToken);
        }
        
        /// <summary>
        /// Executes the specified synchronous action within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        [DebuggerStepThrough]
        public void Execute<T1>(Action<Context, CancellationToken,T1> action, Context context, CancellationToken cancellationToken, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DespatchNonGenericExecution(new SyncExecutableAction<T1>(action, input1), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        [DebuggerStepThrough]
        public void Execute<T1, T2>(Action<Context, CancellationToken, T1, T2> action, Context context, CancellationToken cancellationToken, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DespatchNonGenericExecution(new SyncExecutableAction<T1, T2>(action, input1, input2), context, cancellationToken);
        }

        #region Overloads method-generic in TResult

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> func)
        {
            return DespatchGenericExecution<SyncExecutableFuncNoParams<TResult>, TResult>(
                new SyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<Context, TResult> func, IDictionary<string, object> contextData)
        {
            return DespatchGenericExecution<SyncExecutableFuncOnContext<TResult>, TResult>(
                new SyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken );
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<Context, TResult> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchGenericExecution<SyncExecutableFuncOnContext<TResult>, TResult>(
                new SyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<CancellationToken, TResult> func, CancellationToken cancellationToken)
        {
            return DespatchGenericExecution<SyncExecutableFuncOnCancellationToken<TResult>, TResult>(
                new SyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <param name="func">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<Context, CancellationToken, TResult> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchGenericExecution<SyncExecutableFunc<TResult>, TResult>(
                new SyncExecutableFunc<TResult>(func),
                new Context(contextData),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<Context, CancellationToken, TResult> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchGenericExecution<SyncExecutableFunc<TResult>, TResult>(
                new SyncExecutableFunc<TResult>(func),
                context,
                cancellationToken);
        }
        
        /// <summary>
        /// Executes the specified synchronous function within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<T1, TResult>(Func<Context, CancellationToken, T1, TResult> func, Context context, CancellationToken cancellationToken, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchGenericExecution<ISyncExecutable<TResult>, TResult>(new SyncExecutableFunc<T1,TResult>(func, input1), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The value returned by the function</returns>
        [DebuggerStepThrough]
        public TResult Execute<T1, T2, TResult>(Func<Context, CancellationToken, T1, T2, TResult> func, Context context, CancellationToken cancellationToken, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchGenericExecution<ISyncExecutable<TResult>, TResult>(new SyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken);
        }

        #endregion

        #endregion

        #region ExecuteAndCapture overloads

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action action)
        {
            return DespatchExecuteAndCapture(new SyncExecutableActionNoParams(action), GetDefaultExecutionContext(), DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action<Context> action, IDictionary<string, object> contextData)
        {
            return DespatchExecuteAndCapture(new SyncExecutableActionOnContext(action), new Context(contextData), DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action<Context> action, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture(new SyncExecutableActionOnContext(action), context, DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCapture(new SyncExecutableActionOnCancellationToken(action), GetDefaultExecutionContext(), cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCapture(new SyncExecutableAction(action), new Context(contextData), cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture(new SyncExecutableAction(action), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture<T1>(Action<Context, CancellationToken, T1> action, Context context, CancellationToken cancellationToken, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture(new SyncExecutableAction<T1>(action, input1), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous action within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture<T1, T2>(Action<Context, CancellationToken, T1, T2> action, Context context, CancellationToken cancellationToken, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture(new SyncExecutableAction<T1, T2>(action, input1, input2), context, cancellationToken);
        }

        #region Overloads method-generic in TResult

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> func)
        {
            return DespatchExecuteAndCapture<SyncExecutableFuncNoParams<TResult>, TResult>(
                new SyncExecutableFuncNoParams<TResult>(func),
                GetDefaultExecutionContext(),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> func, IDictionary<string, object> contextData)
        {
            return DespatchExecuteAndCapture<SyncExecutableFuncOnContext<TResult>, TResult>(
                new SyncExecutableFuncOnContext<TResult>(func),
                new Context(contextData),
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> func, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture<SyncExecutableFuncOnContext<TResult>, TResult>(
                new SyncExecutableFuncOnContext<TResult>(func),
                context,
                DefaultCancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> func, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCapture<SyncExecutableFuncOnCancellationToken<TResult>, TResult>(
                new SyncExecutableFuncOnCancellationToken<TResult>(func),
                GetDefaultExecutionContext(),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> func, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return DespatchExecuteAndCapture<SyncExecutableFunc<TResult>, TResult>(
                new SyncExecutableFunc<TResult>(func),
                new Context(contextData),
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="context">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> func, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture<SyncExecutableFunc<TResult>, TResult>(
                new SyncExecutableFunc<TResult>(func),
                context,
                cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>, and returns the captured result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<T1, TResult>(Func<Context, CancellationToken, T1, TResult> func, Context context, CancellationToken cancellationToken, T1 input1)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture<ISyncExecutable<TResult>, TResult>(new SyncExecutableFunc<T1,TResult>(func, input1), context, cancellationToken);
        }

        /// <summary>
        /// Executes the specified synchronous function within the policy, passing two extra inputs of user-defined types <typeparamref name="T1"/> and  <typeparamref name="T2"/>, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <param name="input2">The value of the second custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <typeparam name="T2">The type of the second custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult{TResult}"/></returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<T1, T2, TResult>(Func<Context, CancellationToken, T1, T2, TResult> func, Context context, CancellationToken cancellationToken, T1 input1, T2 input2)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DespatchExecuteAndCapture<ISyncExecutable<TResult>, TResult>(new SyncExecutableFunc<T1, T2, TResult>(func, input1, input2), context, cancellationToken);
        }

        #endregion

        #endregion

    }
}
