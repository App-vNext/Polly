using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Shared
{
    /// <summary>
    /// Static storage for the current collection of policies to report metrics on.
    /// </summary>
    public class CollectedPolicies
    {
        private static WeakDictionary<string, CircuitBreakerPolicy> _policies;
        static CollectedPolicies() {
            _policies = new WeakDictionary<string, CircuitBreakerPolicy>();
        }

        /// <summary>
        /// Return the current list of policies
        /// </summary>
        public static IEnumerable<KeyValuePair<string, CircuitBreakerPolicy>> All
        {
            get { return _policies.All(); }
        }

        /// <summary>
        /// Add a policy to the collection
        /// </summary>
        /// <param name="name">Unique name for this policy</param>
        /// <param name="policy">The policy to record</param>
        public static void Add(string name, CircuitBreakerPolicy policy)
        {
            _policies.Add(name, policy);
        }
    }
}
