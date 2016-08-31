using System;
using System.Linq;
using Polly.CircuitBreaker;
using Polly.Utilities;
using Polly.Shared;

namespace Polly
{
    /// <summary>
    /// Fluent API for adding a policy to metrics. 
    /// </summary>
    public static class CollectMetricsSyntax
    {
        /// <summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <para> Adds policy to metrics collection</para>
        /// </summary>
        /// <param name="name">The policy name.</param>
        /// <returns>The policy instance.</returns>
        public static CircuitBreakerPolicy CollectMetrics(this CircuitBreakerPolicy policyBuilder, string name)
        {
            CollectedPolicies.Add(name, policyBuilder);

            return policyBuilder;
        }
    }
}
