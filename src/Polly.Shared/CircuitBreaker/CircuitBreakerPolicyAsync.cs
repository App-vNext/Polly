#if SUPPORTS_ASYNC
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker
{
    public partial class CircuitBreakerPolicy
    {
        internal CircuitBreakerPolicy(Func<Func<CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates, ICircuitController breakerController)
           : base(asyncExceptionPolicy, exceptionPredicates)
        {
            _breakerController = breakerController;
        }
    }
}
#endif