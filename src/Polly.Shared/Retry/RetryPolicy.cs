using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to delegates.
    /// </summary>
    public partial class RetryPolicy : ContextualPolicy
    {
        internal RetryPolicy(Action<Action, Context> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates) 
            : base(exceptionPolicy, exceptionPredicates)
        {
        }
    }
}