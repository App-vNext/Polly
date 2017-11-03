using System;
using System.Threading;

namespace Polly.Wrap
{
    internal static partial class PolicyWrapEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> func,
            Context context,
            CancellationToken cancellationToken,
            Policy<TResult> outerPolicy,
            Policy<TResult> innerPolicy)
        {
            return outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
           Func<Context, CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           Policy<TResult> outerPolicy,
           Policy innerPolicy)
        {
            return outerPolicy.Execute((ctx, ct) => innerPolicy.Execute<TResult>(func, ctx, ct), context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
           Func<Context, CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           Policy outerPolicy,
           Policy<TResult> innerPolicy)
        {
            return outerPolicy.Execute<TResult>((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
           Func<Context, CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           Policy outerPolicy,
           Policy innerPolicy)
        {
            return outerPolicy.Execute<TResult>((ctx, ct) => innerPolicy.Execute<TResult>(func, ctx, ct), context, cancellationToken);
        }

        internal static void Implementation(
           Action<Context, CancellationToken> action,
           Context context,
           CancellationToken cancellationToken, 
           Policy outerPolicy,
           Policy innerPolicy)
        {
            outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(action, ctx, ct), context, cancellationToken);
        }
    }
}
