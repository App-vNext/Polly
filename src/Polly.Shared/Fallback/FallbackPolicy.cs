using System;
using System.Collections.Generic;
using System.Threading;

namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to delegates.
    /// </summary>
    public partial class FallbackPolicy : Policy
    {
        internal FallbackPolicy(Action<Action<CancellationToken>, Context, CancellationToken> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
            : base(exceptionPolicy, exceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A fallback policy that can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public partial class FallbackPolicy<TResult> : Policy<TResult>
    {
        internal FallbackPolicy(
            Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : base(executionPolicy, exceptionPredicates, resultPredicates)
        {
        }
    }
}