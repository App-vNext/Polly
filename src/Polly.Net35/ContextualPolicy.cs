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
#pragma warning disable CSE0001 //Consider using nameof for the parameter name 'exceptionPolicy'
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");
#pragma warning restore CSE0001

            _exceptionPolicy = exceptionPolicy;
        }

         /// <summary>
         /// Executes the specified action within the policy.
         /// </summary>
         /// <param name="action">The action to perform.</param>
         /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
         /// <exception cref="System.ArgumentNullException">contextData</exception>
#if !DNXCORE50
        [DebuggerStepThrough]
#endif
        public void Execute(Action action, IDictionary<string, object> contextData)
         {
#pragma warning disable CSE0001 //Consider using nameof for the parameter name 'contextData'
            if (contextData == null) throw new ArgumentNullException("contextData");
#pragma warning restore CSE0001

            Execute(action, new Context(contextData));
         }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
#if !DNXCORE50
        [DebuggerStepThrough]
#endif
        public void Execute(Action action)
        {
            Execute(action, Context.Empty);
        }

#if !DNXCORE50
        [DebuggerStepThrough]
#endif
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
#if !DNXCORE50
        [DebuggerStepThrough]
#endif
        public TResult Execute<TResult>(Func<TResult> action, IDictionary<string, object> contextData)
        {
#pragma warning disable CSE0001 //Consider using nameof for the parameter name 'contextData'
            if (contextData == null) throw new ArgumentNullException("contextData");
#pragma warning restore CSE0001

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
#if !DNXCORE50
        [DebuggerStepThrough]
#endif
        public TResult Execute<TResult>(Func<TResult> action)
        {
            return Execute(action, Context.Empty);
        }

#if !DNXCORE50
        [DebuggerStepThrough]
#endif
        private TResult Execute<TResult>(Func<TResult> action, Context context)
        {
            var result = default(TResult);
            _exceptionPolicy(() => { result = action(); }, context);
            return result;
        } 
    }
}