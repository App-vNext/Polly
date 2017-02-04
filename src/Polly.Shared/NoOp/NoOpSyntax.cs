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
        public static NoOpPolicy NoOp()
        {
            return new NoOpPolicy(
                (action, context, cancellationToken) => NoOpEngine.Implementation(
                    ct => { action(ct); return EmptyStruct.Instance; },
                    cancellationToken)
                );
        }
    }
}
