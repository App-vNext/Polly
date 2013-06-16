using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to delegates.
    /// These policies can be called with arbitrary context data.
    /// </summary>
    public class ContextualPolicy
    {
         private readonly Action<Action, Context> _exceptionPolicy;

         internal ContextualPolicy(Action<Action, Context> exceptionPolicy)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");
            
            _exceptionPolicy = exceptionPolicy;
        }

         /// <summary>
         /// Executes the specified action within the policy.
         /// </summary>
         /// <param name="action">The action to perform.</param>
         /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
         /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public void Execute(Action action, IDictionary<string, object> contextData)
         {
             if (contextData == null) throw new ArgumentNullException("contextData");

             Execute(action, new Context(contextData));
         }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            Execute(action, Context.Empty);
        }

        [DebuggerStepThrough]
        private void Execute(Action action, Context context)
        {
            _exceptionPolicy(action, context);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contextData</exception>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action, IDictionary<string, object> contextData)
        {
            if (contextData == null) throw new ArgumentNullException("contextData");

            return Execute(action, new Context(contextData));
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action)
        {
            return Execute(action, Context.Empty);
        }

        [DebuggerStepThrough]
        private TResult Execute<TResult>(Func<TResult> action, Context context)
        {
            var result = default(TResult);
            _exceptionPolicy(() => { result = action(); }, context);
            return result;
        } 
    }
}