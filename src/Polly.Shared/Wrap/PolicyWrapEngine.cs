using System;
using System.Threading;

namespace Polly.Wrap
{
    internal static partial class PolicyWrapEngine
    {
        internal static TResult Implementation<TResult>(
            Func<CancellationToken, TResult> func,
            Context context,
            CancellationToken cancellationToken,
            Policy<TResult> outerPolicy,
            Policy<TResult> innerPolicy)
        {
            return outerPolicy.Execute(ct => innerPolicy.Execute(func, context, ct), context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
           Func<CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           Policy<TResult> outerPolicy,
           Policy innerPolicy)
        {
            return outerPolicy.Execute(ct => innerPolicy.Execute(func, context, ct), context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
           Func<CancellationToken, TResult> func,
           Context context,
           CancellationToken cancellationToken,
           Policy outerPolicy,
           Policy<TResult> innerPolicy)
        {
            return outerPolicy.Execute<TResult>(ct => innerPolicy.Execute(func, context, ct), context, cancellationToken);
        }

        internal static void Implementation(
           Action<CancellationToken> action,
           Context context,
           CancellationToken cancellationToken, 
           Policy outerPolicy,
           Policy innerPolicy)
        {
            outerPolicy.Execute(ct => innerPolicy.Execute(action, context, ct), context, cancellationToken);
        }
    }
}
