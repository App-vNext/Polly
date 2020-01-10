using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle
{
    internal static class AsyncAddBehaviourIfHandleEngine
    {
        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            ExceptionPredicates shouldHandleExceptionPredicates,
            ResultPredicates<TResult> shouldHandleResultPredicates,
            Func<DelegateResult<TResult>, Task> behaviourIfHandle
            )
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            try
            {
                TResult result = await action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);

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
