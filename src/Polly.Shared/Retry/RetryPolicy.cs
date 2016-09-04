using System;
using System.Collections.Generic;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to delegates.
    /// </summary>
    public partial class RetryPolicy : Policy
    {
        internal RetryPolicy(Action<Action, Context> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates) 
            : base(exceptionPolicy, exceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A retry policy that can be applied to delegates returning a value of type <typeparam name="TResult"/>.
    /// </summary>
    public partial class RetryPolicy<TResult> : Policy<TResult>
    {
        internal RetryPolicy(
            Func<Func<TResult>, Context, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : base(executionPolicy, exceptionPredicates, resultPredicates)
        {
        }
    }
}