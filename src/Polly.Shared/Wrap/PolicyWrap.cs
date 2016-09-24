using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Polly.Utilities;

namespace Polly.Wrap
{
    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    public partial class PolicyWrap : Policy
    {
        internal PolicyWrap(Action<Action<CancellationToken>, Context, CancellationToken> policyAction) 
            : base(policyAction, PredicateHelper.EmptyExceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    /// <typeparam name="TResult">The type of the results returned by delegates which may be executed through the policy.</typeparam>
    public partial class PolicyWrap<TResult> : Policy<TResult>
    {
        internal PolicyWrap(Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction)
            : base(policyAction, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }
    }
}
