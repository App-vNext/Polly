using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    internal static class AsyncPolicyWrapEngine
    {
        internal static Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return outerPolicy.ExecuteAsync<IAsyncPolicy<TResult>, TExecutableAsync>(
                (ctx, ct, captureContext, inner, userFunc) => ((IAsyncPolicyInternal<TResult>)inner).ExecuteAsync<TExecutableAsync>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            );
        }

        internal static Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy<TResult> outerPolicy,
            IAsyncPolicy innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return outerPolicy.ExecuteAsync<IAsyncPolicy, TExecutableAsync>(
                (ctx, ct, captureContext, inner, userFunc) => ((IAsyncPolicyInternal)inner).ExecuteAsync<TExecutableAsync, TResult>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            );
        }

        internal static Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy<TResult> innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return outerPolicy.ExecuteAsync<IAsyncPolicy<TResult>, TExecutableAsync, TResult>(
                (ctx, ct, captureContext, inner, userFunc) => ((IAsyncPolicyInternal<TResult>)inner).ExecuteAsync<TExecutableAsync>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            );
        }

        internal static Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync func,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy innerPolicy)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return outerPolicy.ExecuteAsync<IAsyncPolicy, TExecutableAsync, TResult>(
                (ctx, ct, captureContext, inner, userFunc) => ((IAsyncPolicyInternal)inner).ExecuteAsync<TExecutableAsync, TResult>(
                    userFunc,
                    ctx,
                    ct,
                    captureContext
                ),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                func
            );
        }

        internal static Task ImplementationAsync<TExecutableAsync>(
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            IAsyncPolicy outerPolicy,
            IAsyncPolicy innerPolicy)
            where TExecutableAsync : IAsyncExecutable
        {
            return outerPolicy.ExecuteAsync<IAsyncPolicy, TExecutableAsync>(
                (ctx, ct, captureContext, inner, userAction) => ((IAsyncPolicyInternal)inner).ExecuteAsync(
                    userAction,
                    ctx,
                    ct,
                    captureContext
                ),
                context,
                cancellationToken,
                continueOnCapturedContext,
                innerPolicy,
                action
            );
        }
    }
}
