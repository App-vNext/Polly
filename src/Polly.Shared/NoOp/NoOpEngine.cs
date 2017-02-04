using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Polly.NoOp
{
    internal static partial class NoOpEngine
    {
        internal static TResult Implementation<TResult>(Func<CancellationToken, TResult> action,
            CancellationToken cancellationToken)
        {
            return action(cancellationToken);
        }
    }
}
