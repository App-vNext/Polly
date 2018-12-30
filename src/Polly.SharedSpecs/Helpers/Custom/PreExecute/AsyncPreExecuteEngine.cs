using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal static class AsyncPreExecuteEngine
    {
        internal static async Task ImplementationAsync(
            Func<Task> preExecute,
            Func<Context, CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            await (preExecute?.Invoke() ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext);

            await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Task> preExecute,
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            await (preExecute?.Invoke() ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext);

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}