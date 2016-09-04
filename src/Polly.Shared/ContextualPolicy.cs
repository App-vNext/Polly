using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to delegates.
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public void Execute(Action action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));

            Execute(action, new Context(contextData));
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult ExecuteAndCapture(Action action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));

            return ExecuteAndCapture(action, new Context(contextData));
        }
        
        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));

            return Execute(action, new Context(contextData));
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));

            return ExecuteAndCapture(action, new Context(contextData));
        }
    }

    /// <summary>
    /// Transient fault handling policies that can be applied to delegates returning results of type <typeparamref name="TResult"/>
    /// These policies can be called with arbitrary context data.
    /// </summary>
    public partial class Policy<TResult>
    {
        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute(Func<TResult> action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));

            return Execute(action, new Context(contextData));
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        /// <returns>The captured result</returns>
        [DebuggerStepThrough]
        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException(nameof(contextData));

            return ExecuteAndCapture(action, new Context(contextData));
        }
    }
}