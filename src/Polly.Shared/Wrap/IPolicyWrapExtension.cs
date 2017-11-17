using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Wrap
{
    /// <summary>
    /// Extension methods for IPolicyWrap.
    /// </summary>
    public static class IPolicyWrapExtension
    {
        /// <summary>
        /// Recursively iterates through <see cref="PolicyWrap"/> nodes returning <see cref="IsPolicy"/> in Outer-Inner order.
        /// </summary>
        /// <param name="policyWrap"></param>
        /// <returns></returns>
        public static IEnumerable<IsPolicy> GetPolicies(this IPolicyWrap policyWrap)
        {
            var subPolicies = new[] { policyWrap.Outer, policyWrap.Inner };
            foreach (var subPolicy in subPolicies)
            {
                if (subPolicy is IPolicyWrap outerWrap)
                {
                    foreach (var policy in outerWrap.GetPolicies())
                    {
                        yield return policy;
                    }
                }
                else if (subPolicy != null)
                {
                    yield return subPolicy;
                }
            }
        }
    }
}
