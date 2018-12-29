using System;

namespace Polly
{
    public abstract partial class AsyncPolicy
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        public AsyncPolicy WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        IAsyncPolicy IAsyncPolicy.WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

    }

    public abstract partial class AsyncPolicy<TResult>
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        public AsyncPolicy<TResult> WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        IAsyncPolicy<TResult> IAsyncPolicy<TResult>.WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }
    }
}
