using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    internal static partial class PolicyWrapEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<Context, CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
        {
            return await outerPolicy.ExecuteAsync(
                async (ctx, ct) => await innerPolicy.ExecuteAsync(
                    func, 
                    ctx, 
                    ct, 
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext), 
                context, 
                cancellationToken, 
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<Context, CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy innerPolicy)
        {
            return await outerPolicy.ExecuteAsync(
                async (ctx, ct) => await innerPolicy.ExecuteAsync<TResult>(
                    func,
                    ctx,
                    ct,
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
        {
            return await outerPolicy.ExecuteAsync<TResult>(
                async (ctx, ct) => await innerPolicy.ExecuteAsync(
                    func,
                    ctx,
                    ct,
                    continueOnCapturedContext
                    ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<Context, CancellationToken, Task<TResult>> func,
           Context context,
           CancellationToken cancellationToken,
           bool continueOnCapturedContext,
           IAsyncPolicy outerPolicy,
           IAsyncPolicy innerPolicy)
        {
            return await outerPolicy.ExecuteAsync<TResult>(
                async (ctx, ct) => await innerPolicy.ExecuteAsync<TResult>(
                    func,
                    ctx,
                    ct,
                    continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext
            ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task ImplementationAsync(
            Func<Context, CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy innerPolicy)
        {
            await outerPolicy.ExecuteAsync(
                async (ctx, ct) => await innerPolicy.ExecuteAsync(
                    action,
                    ctx,
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
