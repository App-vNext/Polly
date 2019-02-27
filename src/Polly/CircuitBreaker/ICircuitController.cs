using System;

namespace Polly.CircuitBreaker
{
    internal interface ICircuitController<TResult>
    {
        CircuitState CircuitState { get; }
        Exception LastException { get; }
        TResult LastHandledResult { get; }
        void Isolate();
        void Reset();
        void OnCircuitReset(Context context);
        void OnActionPreExecute();
        void OnActionSuccess(Context context);
        void OnActionFailure(DelegateResult<TResult> outcome, Context context);
    }
}
