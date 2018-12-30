using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle
{
    internal static class AsyncAddBehaviourIfHandleEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            ExceptionPredicates shouldHandleExceptionPredicates,
            ResultPredicates<TResult> shouldHandleResultPredicates,
            Func<DelegateResult<TResult>, Task> behaviourIfHandle,
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            try
            {
                TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

                if (shouldHandleResultPredicates.AnyMatch(result))
                {
                    await behaviourIfHandle(new DelegateResult<TResult>(result)).ConfigureAwait(continueOnCapturedContext);
                }

                return result;
            }
            catch (Exception ex)
            {
                Exception handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
                if (handledException == null)
                {
                    throw;
                }

                await behaviourIfHandle(new DelegateResult<TResult>(ex)).ConfigureAwait(continueOnCapturedContext);

                handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
                throw;
            }
        }
    }
}
