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
        public static NoOpPolicy<TResult> NoOp<TResult>()
        {
            return new NoOpPolicy<TResult>(
                (func, context, cancellationToken) => NoOpEngine.Implementation(
                    func,
                    context, 
                    cancellationToken)
                );
        }
    }
}
