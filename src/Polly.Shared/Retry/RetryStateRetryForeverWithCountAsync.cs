using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryStateRetryForeverWithCount<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, int, Context, Task> _onRetryAsync;

        public RetryStateRetryForeverWithCount(Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync, Context context)
        {
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken ct, bool continueOnCapturedContext)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }

            await _onRetryAsync(delegateResult, _errorCount, _context).ConfigureAwait(continueOnCapturedContext);
            return true;
        }
    }
}

