using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    internal static class AsyncPolicyWrapEngineV8
    {
        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return await outerPolicy.ExecuteAsync<IAsyncPolicy<TResult>, TExecutableAsync>(
                async (ctx, ct, captureContext, inner, userFunc) => await ((IAsyncPolicyInternal<TResult>)inner).ExecuteAsync<TExecutableAsync>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return await outerPolicy.ExecuteAsync<IAsyncPolicy, TExecutableAsync>(
                async (ctx, ct, captureContext, inner, userFunc) => await ((IAsyncPolicyInternal)inner).ExecuteAsync<TExecutableAsync, TResult>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return await outerPolicy.ExecuteAsync<IAsyncPolicy<TResult>, TExecutableAsync, TResult>(
                async (ctx, ct, captureContext, inner, userFunc) => await ((IAsyncPolicyInternal<TResult>)inner).ExecuteAsync<TExecutableAsync>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return await outerPolicy.ExecuteAsync<IAsyncPolicy, TExecutableAsync, TResult>(
                async (ctx, ct, captureContext, inner, userFunc) => await ((IAsyncPolicyInternal)inner).ExecuteAsync<TExecutableAsync, TResult>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            ).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task ImplementationAsync(
            IAsyncExecutable action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy innerPolicy)
        {
            await outerPolicy.ExecuteAsync<IAsyncPolicy, IAsyncExecutable>(
                async (ctx, ct, captureContext, inner, userAction) => await ((IAsyncPolicyInternal)inner).ExecuteAsync(
                    userAction,
                    ctx,
                    ct,
                    captureContext
                ).ConfigureAwait(continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                action
            ).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
