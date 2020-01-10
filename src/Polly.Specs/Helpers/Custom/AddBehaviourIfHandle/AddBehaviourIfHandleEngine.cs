using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle
{
    internal static class AddBehaviourIfHandleEngine
    {
        internal static TResult Implementation<TExecutable, TResult>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken,
            ExceptionPredicates shouldHandleExceptionPredicates,
            ResultPredicates<TResult> shouldHandleResultPredicates,
            Action<DelegateResult<TResult>> behaviourIfHandle)
            where TExecutable : ISyncExecutable<TResult>
        {
            try
            {
                TResult result = action.Execute(context, cancellationToken);

                if (shouldHandleResultPredicates.AnyMatch(result))
                {
                    behaviourIfHandle(new DelegateResult<TResult>(result));
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

                behaviourIfHandle(new DelegateResult<TResult>(handledException));

                handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
                throw;
            }
        }
    }
}
