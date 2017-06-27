using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.NoOp
{
    internal static partial class NoOpEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
