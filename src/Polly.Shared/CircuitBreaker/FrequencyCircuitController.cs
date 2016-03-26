using System;

using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class FrequencyCircuitController : CircuitStateController
    {
        private readonly int _faultsAllowedBeforeBreaking;
        private readonly long _measuringPeriodAsTicks;
        private readonly long[] _faultTimings;
        private int _faultIndex;
        private bool _statisticsComplete;

        public FrequencyCircuitController(int faultsAllowedBeforeBreaking, TimeSpan measuringPeriod, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen) : base(durationOfBreak, onBreak, onReset, onHalfOpen)
        {
            _faultsAllowedBeforeBreaking = faultsAllowedBeforeBreaking;
            _measuringPeriodAsTicks = measuringPeriod.Ticks;
            _faultTimings = new long[_faultsAllowedBeforeBreaking];
        }

        public override void OnCircuitReset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _faultIndex = 0;
                _statisticsComplete = false;

                ResetInternal_NeedsLock(context);
            }
        }

        public override void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                if (_circuitState == CircuitState.HalfOpen) { OnCircuitReset(context); }
            }
        }

        public override void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                if (_circuitState == CircuitState.HalfOpen) { Break_NeedsLock(context); }

                long ticksNow = SystemClock.UtcNow().Ticks;
                _faultTimings[_faultIndex] = ticksNow;

                _faultIndex++;
                if (_faultIndex == _faultsAllowedBeforeBreaking)
                {
                    _faultIndex = 0;
                    _statisticsComplete = true;
                }

                if (_statisticsComplete)
                {
                    long ticksNFaultsAgo = _faultTimings[_faultIndex];
                    if (ticksNow - ticksNFaultsAgo <= _measuringPeriodAsTicks)
                    {
                        Break_NeedsLock(context);
                    }
                }
            }
        }
    }
}
