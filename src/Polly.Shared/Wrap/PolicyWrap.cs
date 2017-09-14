using Polly.Utilities;
using System;
using System.Threading;

namespace Polly.Wrap
{
    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    public partial class PolicyWrap : Policy, IPolicyWrap
    {
        internal PolicyWrap(Action<Action<Context, CancellationToken>, Context, CancellationToken> policyAction, ISyncPolicy outer, ISyncPolicy inner)
            : base(policyAction, outer.ExceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public partial class PolicyWrap<TResult> : Policy<TResult>, IPolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction, ISyncPolicy outer, ISyncPolicy<TResult> inner)
            : base(policyAction, outer.ExceptionPredicates,  PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction, ISyncPolicy<TResult> outer, ISyncPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction, ISyncPolicy<TResult> outer, ISyncPolicy<TResult> inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
        }
    }
}
