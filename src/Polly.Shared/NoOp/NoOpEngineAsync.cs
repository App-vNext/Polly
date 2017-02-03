using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.NoOp
{
    internal static partial class NoOpEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(Func<CancellationToken, Task<TResult>> action, 
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
