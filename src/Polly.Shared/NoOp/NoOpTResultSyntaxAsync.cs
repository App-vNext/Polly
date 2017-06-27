using System;
using System.Collections.Generic;
using System.Text;
using Polly.NoOp;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds a NoOp <see cref="Policy"/> that will execute without any custom behavior.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <returns>The policy instance.</returns>
        public static NoOpPolicy<TResult> NoOpAsync<TResult>()
        {
            return new NoOpPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) => NoOpEngine.ImplementationAsync(
                    action,
                    context, 
                    cancellationToken, continueOnCapturedContext)
                );
        }
    }
}
