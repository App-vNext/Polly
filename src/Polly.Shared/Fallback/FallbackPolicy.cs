using System;
using System.Collections.Generic;
using System.Threading;

namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to delegates.
    /// </summary>
    public partial class FallbackPolicy : Policy, IFallbackPolicy
    {
        internal FallbackPolicy(Action<Action<Context, CancellationToken>, Context, CancellationToken> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
            : base(exceptionPolicy, exceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A fallback policy that can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public partial class FallbackPolicy<TResult> : Policy<TResult>, IFallbackPolicy<TResult>
    {
        internal FallbackPolicy(
            Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : base(executionPolicy, exceptionPredicates, resultPredicates)
        {
        }
    }
}