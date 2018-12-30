﻿using System;
using Polly.Utilities;

namespace Polly
{
    public abstract partial class PolicyBase
    {
        /// <summary>
        /// A key intended to be unique to each <see cref="IsPolicy"/> instance.
        /// </summary>
        protected String policyKeyInternal;

        /// <summary>
        /// A key intended to be unique to each <see cref="IsPolicy"/> instance, which is passed with executions as the <see cref="M:Context.PolicyKey"/> property.
        /// </summary>
        public String PolicyKey => policyKeyInternal ?? (policyKeyInternal = GetType().Name + "-" + KeyHelper.GuidPart());

        internal static ArgumentException PolicyKeyMustBeImmutableException => new ArgumentException("PolicyKey cannot be changed once set; or (when using the default value after the PolicyKey property has been accessed.", "policyKey");

        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing policy.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        /// <param name="priorPolicyWrapKey">The <see cref="M:Context.PolicyWrapKey"/> prior to changes by this method.</param>
        /// <param name="priorPolicyKey">The <see cref="M:Context.PolicyKey"/> prior to changes by this method.</param>
        internal virtual void SetPolicyContext(Context executionContext, out string priorPolicyWrapKey, out string priorPolicyKey) // priorPolicyWrapKey and priorPolicyKey could be handled as a (string, string) System.ValueTuple return type instead of out parameters, when our minimum supported target supports this.
        {
            priorPolicyWrapKey = executionContext.PolicyWrapKey;
            priorPolicyKey = executionContext.PolicyKey;

            executionContext.PolicyKey = PolicyKey;
        }

        /// <summary>
        /// Restores the supplied keys to the execution <see cref="Context"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        /// <param name="priorPolicyWrapKey">The <see cref="M:Context.PolicyWrapKey"/> prior to execution through this policy.</param>
        /// <param name="priorPolicyKey">The <see cref="M:Context.PolicyKey"/> prior to execution through this policy.</param>
        internal void RestorePolicyContext(Context executionContext, string priorPolicyWrapKey, string priorPolicyKey)
        {
            executionContext.PolicyWrapKey = priorPolicyWrapKey;
            executionContext.PolicyKey = priorPolicyKey;
        }
    }
}