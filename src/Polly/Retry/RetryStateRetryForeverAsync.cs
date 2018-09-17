using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryStateRetryForever<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, Context, Task> _onRetryAsync;

        public RetryStateRetryForever(Func<DelegateResult<TResult>, Context, Task> onRetryAsync, Context context)
        {
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken ct, bool continueOnCapturedContext)
        {
            await _onRetryAsync(delegateResult, _context).ConfigureAwait(continueOnCapturedContext);
            return true;
        }
    }
}

