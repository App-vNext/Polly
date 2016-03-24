using System;

namespace Polly.CircuitBreaker
{
    internal interface ICircuitController
    {
        CircuitState CircuitState { get; }
        Exception LastException { get; }
        void Isolate();
        void Reset();
        void OnCircuitReset();
        void OnActionPreExecute();
        void OnActionSuccess(Context context);
        void OnActionFailure(Exception ex, Context context);
    }
}
