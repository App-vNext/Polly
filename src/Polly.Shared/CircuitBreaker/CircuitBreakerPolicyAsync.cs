#if SUPPORTS_ASYNC
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    public partial class CircuitBreakerPolicy
    {
        internal CircuitBreakerPolicy(
            Func<Func<CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates, 
            ICircuitController<EmptyStruct> breakerController
            ) : base(asyncExceptionPolicy, exceptionPredicates)
        {
            _breakerController = breakerController;
        }
    }

    public partial class CircuitBreakerPolicy<TResult>
    {
        internal CircuitBreakerPolicy(
            Func<Func<CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy, 
            IEnumerable<ExceptionPredicate> exceptionPredicates, 
            IEnumerable<ResultPredicate<TResult>> resultPredicates, 
            ICircuitController<TResult> breakerController
            ) : base(asyncExecutionPolicy, exceptionPredicates, resultPredicates)
        {
            _breakerController = breakerController;
        }
    }
}
#endif