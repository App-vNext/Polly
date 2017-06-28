using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Fallback
{
    public partial class FallbackPolicy : IFallbackPolicy
    {
        internal FallbackPolicy(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
           : base(asyncExceptionPolicy, exceptionPredicates)
        {
        }
    }

    public partial class FallbackPolicy<TResult> : IFallbackPolicy<TResult>
    {
        internal FallbackPolicy(
            Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy,
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : base(asyncExecutionPolicy, exceptionPredicates, resultPredicates)
        {
        }
    }
}