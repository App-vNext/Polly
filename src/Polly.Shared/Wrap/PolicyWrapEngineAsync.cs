using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    internal static partial class PolicyWrapEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            Policy<TResult> outerPolicy,
            Policy<TResult> innerPolicy)
        {
            return await outerPolicy.ExecuteAsync(
                async ct => await innerPolicy.ExecuteAsync(
                    func, 
                    context, 
                    ct, 
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext), 
                context, 
                cancellationToken, 
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            Policy<TResult> outerPolicy,
            Policy innerPolicy)
        {
            return await outerPolicy.ExecuteAsync(
                async ct => await innerPolicy.ExecuteAsync(
                    func,
                    context,
                    ct,
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            Policy outerPolicy,
            Policy<TResult> innerPolicy)
        {
            return await outerPolicy.ExecuteAsync(
                async ct => await innerPolicy.ExecuteAsync(
                    func,
                    context,
                    ct,
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task ImplementationAsync(
            Func<CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            Policy outerPolicy,
            Policy innerPolicy)
        {
            await outerPolicy.ExecuteAsync(
                async ct => await innerPolicy.ExecuteAsync(
                    action,
                    context,
                    ct,
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

    }
}
