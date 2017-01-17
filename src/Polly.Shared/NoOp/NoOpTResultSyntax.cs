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
        public static NoOpPolicy<TResult> NoOp<TResult>()
        {
            return new NoOpPolicy<TResult>(
                (func, context, cancellationToken) => NoOpEngine.Implementation(
                    func,
                    cancellationToken)
                );
        }
    }
}
