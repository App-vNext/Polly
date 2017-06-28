using System;
using System.Collections.Generic;
using System.Text;
using Polly.NoOp;
using Polly.Utilities;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds a NoOp <see cref="Policy"/> that will execute without any custom behavior.
        /// </summary>
        /// <returns>The policy instance.</returns>
        public static NoOpPolicy NoOpAsync()
        {
            return new NoOpPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => NoOpEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context, cancellationToken, continueOnCapturedContext)
                );
        }
    }
}
