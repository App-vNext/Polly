using System;
using Polly.Utilities;

namespace Polly
{
    public partial class Policy
    {
        private String _policyKey;

        /// <summary>
        /// A key intended to be unique to each <see cref="Policy"/> instance, which is passed with executions as the <see cref="M:Context.PolicyKey"/> property.
        /// </summary>
        public String PolicyKey => _policyKey ?? (_policyKey = GetType().Name + "-" + KeyHelper.GuidPart());

        internal static ArgumentException PolicyKeyMustBeImmutableException => new ArgumentException("PolicyKey cannot be changed once set; or (when using the default value after the PolicyKey property has been accessed.", "policyKey");

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        public Policy WithPolicyKey(String policyKey)
        {
            if (_policyKey != null) throw PolicyKeyMustBeImmutableException;

            _policyKey = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        ISyncPolicy ISyncPolicy.WithPolicyKey(String policyKey)
        {
            if (_policyKey != null) throw PolicyKeyMustBeImmutableException;

            _policyKey = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        IAsyncPolicy IAsyncPolicy.WithPolicyKey(String policyKey)
        {
            if (_policyKey != null) throw PolicyKeyMustBeImmutableException;

            _policyKey = policyKey;
            return this;
        }

        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing <see cref="Policy"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        internal virtual void SetPolicyContext(Context executionContext)
        {
            executionContext.PolicyKey = PolicyKey;
        }
    }

    public partial class Policy<TResult>
    {
        private String _policyKey;

        /// <summary>
        /// A key intended to be unique to each <see cref="Policy"/> instance, which is passed with executions as the <see cref="M:Context.PolicyKey"/> property.
        /// </summary>
        public String PolicyKey => _policyKey ?? (_policyKey = GetType().Name + "-" + KeyHelper.GuidPart());

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        public Policy<TResult> WithPolicyKey(String policyKey)
        {
            if (_policyKey != null) throw Policy.PolicyKeyMustBeImmutableException;

            _policyKey = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        ISyncPolicy<TResult> ISyncPolicy<TResult>.WithPolicyKey(String policyKey)
        {
            if (_policyKey != null) throw Policy.PolicyKeyMustBeImmutableException;

            _policyKey = policyKey;
            return this;
        }

        /// <summary>
        /// Sets the PolicyKey for this <see cref="Policy"/> instance.
        /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
        /// </summary>
        /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
        IAsyncPolicy<TResult> IAsyncPolicy<TResult>.WithPolicyKey(String policyKey)
        {
            if (_policyKey != null) throw Policy.PolicyKeyMustBeImmutableException;

            _policyKey = policyKey;
            return this;
        }
        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing <see cref="Policy{TResult}"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        internal virtual void SetPolicyContext(Context executionContext)
        {
            executionContext.PolicyKey = PolicyKey;
        }
    }
}
