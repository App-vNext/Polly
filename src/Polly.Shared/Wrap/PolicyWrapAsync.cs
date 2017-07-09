using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Wrap
{
    public partial class PolicyWrap : IPolicyWrap
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> policyAction, Policy outer, Policy inner)
            : base(policyAction, outer.ExceptionPredicates)
        {
        }
    }

    public partial class PolicyWrap<TResult> : IPolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, Policy outer, IsPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, Policy<TResult> outer, IsPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
        }
    }
}
