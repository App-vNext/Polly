using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Wrap
{
    public partial class PolicyWrap
    {
        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing <see cref="PolicyWrap"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        internal override void SetPolicyContext(Context executionContext)
        {
            if (executionContext.PolicyWrapKey == null) executionContext.PolicyWrapKey = PolicyKey;

            base.SetPolicyContext(executionContext);
        }
    }

    public partial class PolicyWrap<TResult>
    {
        /// <summary>
        /// Updates the execution <see cref="Context"/> with context from the executing <see cref="PolicyWrap{TResult}"/>.
        /// </summary>
        /// <param name="executionContext">The execution <see cref="Context"/>.</param>
        internal override void SetPolicyContext(Context executionContext)
        {
            if (executionContext.PolicyWrapKey == null) executionContext.PolicyWrapKey = PolicyKey;

            base.SetPolicyContext(executionContext);
        }
    }
}
