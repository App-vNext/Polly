using System;
using System.Collections.Generic;
using System.Threading;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to delegates.
    /// </summary>
    public partial class RetryPolicy : Policy
    {
        internal RetryPolicy(Action<Action<CancellationToken>, Context, CancellationToken> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates) 
            : base(exceptionPolicy, exceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A retry policy that can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public partial class RetryPolicy<TResult> : Policy<TResult>
    {
        internal RetryPolicy(
            Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : base(executionPolicy, exceptionPredicates, resultPredicates)
        {
        }
    }
}