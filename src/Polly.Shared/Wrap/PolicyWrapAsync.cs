using Polly.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    public partial class PolicyWrap : IPolicyWrap
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> policyAction, IAsyncPolicy outer, IAsyncPolicy inner)
            : base(policyAction, outer.ExceptionPredicates)
        {
        }
    }

    public partial class PolicyWrap<TResult> : IPolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, IAsyncPolicy outer, IAsyncPolicy<TResult> inner)
            : base(policyAction, outer.ExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, IAsyncPolicy<TResult> outer, IAsyncPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, IAsyncPolicy<TResult> outer, IAsyncPolicy<TResult> inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
        }
    }
}
