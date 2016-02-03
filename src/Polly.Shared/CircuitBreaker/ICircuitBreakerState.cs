using System;

namespace Polly.CircuitBreaker
{
    internal interface ICircuitBreakerState
    {
        CircuitState CircuitState { get; }
        Exception LastException { get; }
        void Isolate();
        void Reset();
        void OnActionSuccess(Context context);
        void OnActionFailure(Exception ex, Context context);
    }
}