using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Wrap
{
    public partial class PolicyWrap : IPolicyWrap
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> policyAction)
            : base(policyAction, PredicateHelper.EmptyExceptionPredicates)
        {
        }
    }

    public partial class PolicyWrap<TResult> : IPolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction)
            : base(policyAction, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }
    }
}
