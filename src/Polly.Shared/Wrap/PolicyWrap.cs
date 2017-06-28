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
    public partial class PolicyWrap : Policy, IPolicyWrap
    {
        internal PolicyWrap(Action<Action<Context, CancellationToken>, Context, CancellationToken> policyAction) 
            : base(policyAction, PredicateHelper.EmptyExceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public partial class PolicyWrap<TResult> : Policy<TResult>, IPolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction)
            : base(policyAction, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }
    }
}
