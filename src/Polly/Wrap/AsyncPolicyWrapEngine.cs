using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    internal static class AsyncPolicyWrapEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<Context, CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
            => await outerPolicy.ExecuteAsync(
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

        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<Context, CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy innerPolicy)
            => await outerPolicy.ExecuteAsync(
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

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
            => await outerPolicy.ExecuteAsync<TResult>(
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

        internal static async Task<TResult> ImplementationAsync<TResult>(
           Func<Context, CancellationToken, Task<TResult>> func,
           Context context,
           CancellationToken cancellationToken,
           bool continueOnCapturedContext,
           IAsyncPolicy outerPolicy,
           IAsyncPolicy innerPolicy)
            => await outerPolicy.ExecuteAsync<TResult>(
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

        internal static async Task ImplementationAsync(
            Func<Context, CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy innerPolicy)
            => await outerPolicy.ExecuteAsync(
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
