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

        public override void OnCircuitReset()
        {
            _faultIndex = 0;
            _statisticsComplete = false;
        }

        public override void OnActionSuccess(Context context) { }

        public override void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                long ticksNow = SystemClock.UtcNow().Ticks;
                _faultTimings[_faultIndex] = ticksNow;

                _faultIndex = (_faultIndex + 1) %_faultsAllowedBeforeBreaking;
                _statisticsComplete = _statisticsComplete || (_faultIndex == 0); // If at 0 after ++ % operation, we must have wrapped.

                // Alternative to above - more readable?
                //_faultIndex++;
                //if (_faultIndex == _faultsAllowedBeforeBreaking)
                //{
                //    _statisticsComplete = true;
                //    _faultIndex = 0;
                //}

                if (_statisticsComplete)
                {
                    long ticksNFaultsAgo = _faultTimings[_faultIndex];
                    if (ticksNow - ticksNFaultsAgo <= _measuringPeriodAsTicks)
                    {
                        Break(context);
                    }
                }
            }
        }

    }
}
