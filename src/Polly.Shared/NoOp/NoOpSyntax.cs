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
