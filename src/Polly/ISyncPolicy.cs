using System;
using System.Collections.Generic;
using System.Threading;

namespace Polly
{
    /// <summary>
    /// An interface defining all executions available on a non-generic, synchronous policy
    /// </summary>
    public interface ISyncPolicy : IsPolicy
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        ISyncPolicy WithPolicyKey(String policyKey);

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        void Execute(Action action);

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        void Execute(Action<Context> action, IDictionary<string, object> contextData);

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        void Execute(Action<Context> action, Context context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        void Execute(Action<CancellationToken> action, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException">contextData</exception>
        void Execute(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void Execute(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified synchronous action within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        void Execute<T1>(Action<Context, CancellationToken,T1> action, Context context, CancellationToken cancellationToken, T1 input1);

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
        void Execute<T1, T2>(Action<Context, CancellationToken, T1, T2> action, Context context, CancellationToken cancellationToken, T1 input1, T2 input2);

        /// <summary>
        /// Executes the specified action within the policy and returns the Result.
        /// </summary>
        /// <typeparam name="TResult">The type of the Result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        TResult Execute<TResult>(Func<TResult> action);

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="ArgumentNullException">contextData</exception>
        TResult Execute<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData);

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <exception cref="ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="ArgumentNullException">contextData</exception>
        TResult Execute<TResult>(Func<Context, TResult> action, Context context);

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="ArgumentNullException">contextData</exception>
        TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action</returns>
        TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken);

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
        TResult Execute<T1, TResult>(Func<Context, CancellationToken, T1, TResult> func, Context context, CancellationToken cancellationToken, T1 input1);

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
        TResult Execute<T1, T2, TResult>(Func<Context, CancellationToken, T1, T2, TResult> func, Context context, CancellationToken cancellationToken, T1 input1, T2 input2);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        PolicyResult ExecuteAndCapture(Action action);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        PolicyResult ExecuteAndCapture(Action<Context> action, IDictionary<string, object> contextData);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <returns>The captured result</returns>
        PolicyResult ExecuteAndCapture(Action<Context> action, Context context);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="ArgumentNullException">contextData</exception>
        PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified synchronous action within the policy, passing an extra input of user-defined type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <param name="input1">The value of the first custom input to the function.</param>
        /// <typeparam name="T1">The type of the first custom input to the function.</typeparam>
        /// <returns>The outcome of the execution, as a captured <see cref="PolicyResult"/></returns>
        PolicyResult ExecuteAndCapture<T1>(Action<Context, CancellationToken, T1> action, Context context, CancellationToken cancellationToken, T1 input1);

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
        PolicyResult ExecuteAndCapture<T1, T2>(Action<Context, CancellationToken, T1, T2> action, Context context, CancellationToken cancellationToken, T1 input1, T2 input2);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The captured result</returns>
        PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <exception cref="ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, Context context);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        /// <exception cref="ArgumentNullException">contextData</exception>
        PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Context data that is passed to the exception policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The captured result</returns>
        PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken);

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
        PolicyResult<TResult> ExecuteAndCapture<T1, TResult>(Func<Context, CancellationToken, T1, TResult> func, Context context, CancellationToken cancellationToken, T1 input1);

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
        PolicyResult<TResult> ExecuteAndCapture<T1, T2, TResult>(Func<Context, CancellationToken, T1, T2, TResult> func, Context context, CancellationToken cancellationToken, T1 input1, T2 input2);
    }
}
