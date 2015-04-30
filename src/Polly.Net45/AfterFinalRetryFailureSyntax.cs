using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining an action to run after the final Retry. 
    /// </summary>
    public static partial class AfterFinalRetryFailureSyntax
    {
        /// <summary>
        /// Sets the action to run after the final retry
        /// </summary>
        /// <returns></returns>
        public static PolicyBuilder AfterFinalRetryFailure(this PolicyBuilder policyBuilder, Action<Exception> action)
        {
            policyBuilder.AfterFinalRetryFailureAction = action;
            return policyBuilder;
        }

    }
}
