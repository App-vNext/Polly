using System;

namespace Polly
{
    public abstract partial class PolicyV8
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="PolicyV8"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="PolicyV8"/> instance.</param>
        public PolicyV8 WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="ISyncPolicy"/> instance.</param>
        ISyncPolicy ISyncPolicy.WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }
    }

    public abstract partial class PolicyV8<TResult>
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="PolicyV8{TResult}"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="PolicyV8{TResult}"/> instance.</param>
        public PolicyV8<TResult> WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy{TResult}"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy{TResult}"/> instance.</param>
        ISyncPolicy<TResult> ISyncPolicy<TResult>.WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }
    }
}
