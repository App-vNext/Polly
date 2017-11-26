using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

namespace Polly.Fallback
{
    internal static partial class FallbackEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates,
            Func<DelegateResult<TResult>, Context, Task> onFallbackAsync,
            Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            DelegateResult<TResult> delegateOutcome;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

                if (!shouldHandleResultPredicates.Any(predicate => predicate(result)))
                {
                    return result;
                }

                delegateOutcome = new DelegateResult<TResult>(result);
            }
            catch (Exception ex)
            {
                Exception handledException = shouldHandleExceptionPredicates
                    .Select(predicate => predicate(ex))
                    .FirstOrDefault(e => e != null);
                if (handledException == null)
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(handledException);
            }

            await onFallbackAsync(delegateOutcome, context).ConfigureAwait(continueOnCapturedContext);

            return await fallbackAction(delegateOutcome, context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
