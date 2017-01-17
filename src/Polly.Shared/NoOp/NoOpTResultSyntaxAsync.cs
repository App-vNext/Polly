using System;
using System.Collections.Generic;
using System.Text;
using Polly.Shared.NoOp;

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
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static NoOpPolicy<TResult> NoOpAsync<TResult>()
        {
            return new NoOpPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) => NoOpEngine.ImplementationAsync(
                    action,
                    cancellationToken)
                );
        }
    }
}
