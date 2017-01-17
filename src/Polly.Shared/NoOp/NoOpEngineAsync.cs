using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Shared.NoOp
{
    internal static partial class NoOpEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DelegateResult<TResult> delegateOutcome = 
                new DelegateResult<TResult>(await action(cancellationToken));

            return delegateOutcome.Result;
        }
    }
}
