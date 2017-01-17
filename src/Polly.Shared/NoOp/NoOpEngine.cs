using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Polly.Shared.NoOp
{
    internal static partial class NoOpEngine
    {
        internal static TResult Implementation<TResult>(Func<CancellationToken, TResult> action,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DelegateResult<TResult> delegateOutcome = 
                new DelegateResult<TResult>(action(cancellationToken));

            return delegateOutcome.Result;
        }
    }
}
