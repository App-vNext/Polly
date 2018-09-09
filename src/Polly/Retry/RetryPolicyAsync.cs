using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    public partial class RetryPolicy : IRetryPolicy
    {
        internal RetryPolicy(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
           : base(asyncExceptionPolicy, exceptionPredicates)
        {
        }
    }

    public partial class RetryPolicy<TResult> : IRetryPolicy<TResult>
    {
        internal RetryPolicy(
            Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> resultPredicates
            ) : base(asyncExecutionPolicy, exceptionPredicates, resultPredicates)
        {
        }
    }
}

