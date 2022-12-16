using System;
using System.Threading;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal static class PreExecuteEngine
    {
        internal static void Implementation(
            Action preExecute,
            Action<Context, CancellationToken> action, 
            Context context, 
            CancellationToken cancellationToken)
        {
            preExecute?.Invoke();

            action(context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
            Action preExecute,
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken)
        {
            preExecute?.Invoke();

            return action(context, cancellationToken);
        }
    }
}