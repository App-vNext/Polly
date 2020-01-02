using System;

namespace Polly
{
    public abstract partial class AsyncPolicyV8
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="AsyncPolicyV8"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="AsyncPolicyV8"/> instance.</param>
        public AsyncPolicyV8 WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="IAsyncPolicy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="IAsyncPolicy"/> instance.</param>
        IAsyncPolicy IAsyncPolicy.WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

    }

    public abstract partial class AsyncPolicyV8<TResult>
    {
        /// <summary>
        /// Sets the PolicyKey for this <see cref="AsyncPolicyV8{TResult}"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="AsyncPolicyV8{TResult}"/> instance.</param>
        public AsyncPolicyV8<TResult> WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="IAsyncPolicy{TResult}"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="IAsyncPolicy{TResult}"/> instance.</param>
        IAsyncPolicy<TResult> IAsyncPolicy<TResult>.WithPolicyKey(String policyKey)
        {
            if (policyKeyInternal != null) throw PolicyKeyMustBeImmutableException;

            policyKeyInternal = policyKey;
            return this;
        }
    }
}
