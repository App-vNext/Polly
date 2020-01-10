using System;
using System.Threading;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal static class PreExecuteEngine
    {
        internal static TResult Implementation<TExecutable, TResult>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken,
            Action preExecute)
            where TExecutable : ISyncExecutable<TResult>
        {
            preExecute?.Invoke();

            return action.Execute(context, cancellationToken);
        }
    }
}