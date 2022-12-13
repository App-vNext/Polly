using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;

internal static class AddBehaviourIfHandleEngine
{
    internal static TResult Implementation<TResult>(
        ExceptionPredicates shouldHandleExceptionPredicates,
        ResultPredicates<TResult> shouldHandleResultPredicates,
        Action<DelegateResult<TResult>> behaviourIfHandle,
        Func<Context, CancellationToken, TResult> action,
        Context context,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = action(context, cancellationToken);

            if (shouldHandleResultPredicates.AnyMatch(result))
            {
                behaviourIfHandle(new DelegateResult<TResult>(result));
            }

            return result;
        }
        catch (Exception ex)
        {
            var handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
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