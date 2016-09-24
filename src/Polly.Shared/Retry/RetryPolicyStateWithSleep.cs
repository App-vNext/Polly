﻿using System;
using System.Collections.Generic;
using System.Threading;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleep<TResult> : IRetryPolicyState<TResult>
    {
        private int _errorCount;
        private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;
        private readonly Context _context;
        private readonly IEnumerator<TimeSpan> _sleepDurationsEnumerator;

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry, Context context) :
            this(sleepDurations, (delegateResult, span, i, c) => onRetry(delegateResult, span, c), context)
        {
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            _errorCount += 1;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            _onRetry(delegateResult, currentTimeSpan, _errorCount, _context);
                
            SystemClock.Sleep(currentTimeSpan, cancellationToken);

            return true;
        }
    }
}