using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal static class AsyncPreExecuteEngine
    {
        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            Func<Task> preExecute)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            await (preExecute?.Invoke() ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext);

            return await action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
    }
}