#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryPolicyState : IRetryPolicyState
    {
        private readonly Func<Exception, Context, Task> _onRetryAsync;

        public RetryPolicyState(Func<Exception, Context, Task> onRetryAsync, Context context)
        {
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public RetryPolicyState(Func<Exception, Task> onRetryAsync) :
            this((exception, context) => onRetryAsync(exception), Context.Empty)
        {
        }

        public async Task<bool> CanRetryAsync(Exception ex, CancellationToken ct, bool continueOnCapturedContext)
        {
            await _onRetryAsync(ex, _context).ConfigureAwait(continueOnCapturedContext);
            return true;
        }
    }
}

#endif