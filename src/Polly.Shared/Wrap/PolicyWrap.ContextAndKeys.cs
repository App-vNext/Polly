﻿namespace Polly.Wrap
{
    public partial class PolicyWrap
    {
        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing <see cref="PolicyWrap"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        /// <param name="priorPolicyWrapKey">The <see cref="M:Context.PolicyWrapKey"/> prior to changes by this method.</param>
        /// <param name="priorPolicyKey">The <see cref="M:Context.PolicyKey"/> prior to changes by this method.</param>
        internal override void SetPolicyContext(Context executionContext, out string priorPolicyWrapKey, out string priorPolicyKey)
        {
            priorPolicyWrapKey = executionContext.PolicyWrapKey;
            priorPolicyKey = executionContext.PolicyKey;

            if (executionContext.PolicyWrapKey == null) executionContext.PolicyWrapKey = PolicyKey;

            base.SetPolicyContext(executionContext, out _, out _);
        }
    }

    public partial class PolicyWrap<TResult>
    {
        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing <see cref="PolicyWrap{TResult}"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        /// <param name="priorPolicyWrapKey">The <see cref="M:Context.PolicyWrapKey"/> prior to changes by this method.</param>
        /// <param name="priorPolicyKey">The <see cref="M:Context.PolicyKey"/> prior to changes by this method.</param>
        internal override void SetPolicyContext(Context executionContext, out string priorPolicyWrapKey, out string priorPolicyKey)
        {
            priorPolicyWrapKey = executionContext.PolicyWrapKey;
            priorPolicyKey = executionContext.PolicyKey;

            if (executionContext.PolicyWrapKey == null) executionContext.PolicyWrapKey = PolicyKey;

            base.SetPolicyContext(executionContext, out _, out _);
        }
    }
}
