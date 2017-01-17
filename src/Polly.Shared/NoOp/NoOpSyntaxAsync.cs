using System;
using System.Collections.Generic;
using System.Text;
using Polly.Shared.NoOp;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static NoOpPolicy NoOpAsync()
        {
            return new NoOpPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => NoOpEngine.ImplementationAsync(
                    async ct => { await action(ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    cancellationToken)
                );
        }
    }
}
