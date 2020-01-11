using System.Threading;

namespace Polly.Wrap
{
    internal static class PolicyWrapEngineV8
    {
        internal static TResult Implementation<TExecutable, TResult>(
            TExecutable func,
            Context context,
            CancellationToken cancellationToken,
            ISyncPolicy<TResult> outerPolicy,
            ISyncPolicy<TResult> innerPolicy)
            where TExecutable : ISyncExecutable<TResult>
        {
            return outerPolicy.Execute<ISyncPolicy<TResult>, TExecutable>(
                (ctx, ct, inner, userFunc) => ((ISyncPolicyInternal<TResult>)inner).Execute<TExecutable>(userFunc, ctx, ct), 
                context, 
                cancellationToken,
                innerPolicy,
                func);
        }

        internal static TResult Implementation<TExecutable, TResult>(
            TExecutable func,
            Context context,
            CancellationToken cancellationToken,
            ISyncPolicy<TResult> outerPolicy,
            ISyncPolicy innerPolicy)
            where TExecutable : ISyncExecutable<TResult>
        {
            return outerPolicy.Execute<ISyncPolicy, TExecutable>(
                (ctx, ct, inner, userFunc) => ((ISyncPolicyInternal)inner).Execute<TExecutable, TResult>(userFunc, ctx, ct), 
                context, 
                cancellationToken,
                innerPolicy,
                func);
        }

        internal static TResult Implementation<TExecutable, TResult>(
            TExecutable func,
            Context context,
            CancellationToken cancellationToken,
            ISyncPolicy outerPolicy,
            ISyncPolicy<TResult> innerPolicy) 
            where TExecutable : ISyncExecutable<TResult>
        {
            return outerPolicy.Execute<ISyncPolicy<TResult>, TExecutable, TResult>(
                (ctx, ct, inner, userFunc) => ((ISyncPolicyInternal<TResult>)inner).Execute<TExecutable>(userFunc, ctx, ct), 
                context, 
                cancellationToken,
                innerPolicy,
                func);
        }

        internal static TResult Implementation<TExecutable, TResult>(
            TExecutable func,
            Context context,
            CancellationToken cancellationToken,
            ISyncPolicy outerPolicy,
            ISyncPolicy innerPolicy)
            where TExecutable : ISyncExecutable<TResult>
        {
            return outerPolicy.Execute<ISyncPolicy, TExecutable, TResult>(
                (ctx, ct, inner, userFunc) => ((ISyncPolicyInternal) inner).Execute<TExecutable, TResult>(userFunc, ctx, ct),
                context,
                cancellationToken,
                innerPolicy,
                func);
        }

        internal static void Implementation(
            ISyncExecutable action,
            Context context,
            CancellationToken cancellationToken, 
            ISyncPolicy outerPolicy,
            ISyncPolicy innerPolicy)
        {
            outerPolicy.Execute<ISyncPolicy, ISyncExecutable>(
                (ctx, ct, inner, userAction) => ((ISyncPolicyInternal) inner).Execute(userAction, ctx, ct),
                context,
                cancellationToken,
                innerPolicy,
                action);
        }
    }
}
