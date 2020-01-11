using System;
using System.Threading;

namespace Polly.Wrap
{
    internal static class PolicyWrapEngineV8
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> func,
            Context context,
            CancellationToken cancellationToken,
            ISyncPolicy<TResult> outerPolicy,
            ISyncPolicy<TResult> innerPolicy)
            => outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);

        internal static TResult Implementation<TResult>(
           Func<Context, CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           ISyncPolicy<TResult> outerPolicy,
           ISyncPolicy innerPolicy)
            =>  outerPolicy.Execute((ctx, ct) => innerPolicy.Execute<TResult>(func, ctx, ct), context, cancellationToken);

        internal static TResult Implementation<TResult>(
           Func<Context, CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           ISyncPolicy outerPolicy,
           ISyncPolicy<TResult> innerPolicy)
            => outerPolicy.Execute<TResult>((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);

        internal static TResult Implementation<TResult>(
           Func<Context, CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           ISyncPolicy outerPolicy,
           ISyncPolicy innerPolicy)
            => outerPolicy.Execute<TResult>((ctx, ct) => innerPolicy.Execute<TResult>(func, ctx, ct), context, cancellationToken);

        internal static TResult Implementation<TExecutable, TResult>(
            TExecutable func,
            Context context,
            CancellationToken cancellationToken,
            ISyncPolicy outerPolicy,
            ISyncPolicy innerPolicy)
            where TExecutable : ISyncExecutable<TResult>
        {
            return outerPolicy.Execute(
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
            outerPolicy.Execute(
                (ctx, ct, inner, userAction) => ((ISyncPolicyInternal) inner).Execute(userAction, ctx, ct),
                context,
                cancellationToken,
                innerPolicy,
                action);
        }
    }
}
