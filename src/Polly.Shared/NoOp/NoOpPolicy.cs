using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Shared.NoOp
{
    /// <summary>
    /// A no op policy that can be applied to delegates.
    /// </summary>
    public partial class NoOpPolicy : Policy
    {
        internal NoOpPolicy(Action<Action<CancellationToken>, Context, CancellationToken> exceptionPolicy)
            : base(exceptionPolicy, PredicateHelper.EmptyExceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A no op policy that can be applied to delegates returning a value of type <typeparamref name="TResult" />
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public partial class NoOpPolicy<TResult> : Policy<TResult>
    {
        internal NoOpPolicy(
             Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy
             ) : base(executionPolicy, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }
    }
}
