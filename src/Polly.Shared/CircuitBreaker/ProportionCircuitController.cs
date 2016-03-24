using System;

using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class ProportionCircuitController : CircuitStateController
    {
        private readonly int _faultsAllowedBeforeBreaking;
        private readonly int _perTotalActions;
        private readonly bool[] _faultHistory;
        private int _faultCount;
        private int _historyIndex;

        public ProportionCircuitController(int faultsAllowedBeforeBreaking, int perTotalActions, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen) : base(durationOfBreak, onBreak, onReset, onHalfOpen)
        {
            _faultsAllowedBeforeBreaking = faultsAllowedBeforeBreaking;
            _perTotalActions = perTotalActions;
            _faultHistory = new bool[_perTotalActions];
        }
        
        public override void OnCircuitReset()
        {
            for (int i = 0; i < _perTotalActions; i++) _faultHistory[i] = false;
            _faultCount = 0;
            _historyIndex = 0;
        }

        public override void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                if (_circuitState == CircuitState.HalfOpen) { Reset(context); }

                if (_faultHistory[_historyIndex]) _faultCount--; // We have success, this slot previously recorded a failure, so we are now one failure less.
                _faultHistory[_historyIndex] = false;
                _historyIndex = (_historyIndex + 1) % _perTotalActions;
            }
        }

        public override void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                if (!_faultHistory[_historyIndex]) _faultCount++; // We have failure, this slot previously recorded a success, so we are now one failure more.
                _faultHistory[_historyIndex] = true;
                _historyIndex = (_historyIndex + 1) % _perTotalActions;

                if (_faultCount >= _faultsAllowedBeforeBreaking)
                {
                    Break_NeedsLock(context);
                }
            }
        }
    }
}
