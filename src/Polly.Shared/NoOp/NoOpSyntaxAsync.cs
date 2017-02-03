using System;
using System.Collections.Generic;
using System.Text;
using Polly.NoOp;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a NoOp <see cref="Policy"/>.
    /// </summary>
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
                    async ct => { await action(ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    cancellationToken, continueOnCapturedContext)
                );
        }
    }
}
