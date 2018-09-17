using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Polly.NoOp
{
    internal static partial class NoOpEngine
    {
        internal static TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return action(context, cancellationToken);
        }
    }
}
