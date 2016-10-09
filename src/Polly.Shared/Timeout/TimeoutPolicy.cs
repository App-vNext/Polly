using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Timeout
{
    /// <summary>
    /// A timeout policy which can be applied to delegates.
    /// </summary>
    public partial class TimeoutPolicy : Policy
    {
        internal TimeoutPolicy(
            Action<Action<CancellationToken>, Context, CancellationToken> exceptionPolicy
            ) 
            : base(exceptionPolicy, PredicateHelper.EmptyExceptionPredicates)
        {

        }
    }

    /// <summary>
    /// A timeout policy which can be applied to delegates returning a value of type <typeparam name="TResult"/>.
    /// </summary>
    public partial class TimeoutPolicy<TResult> : Policy<TResult>
    {
        internal TimeoutPolicy(
            Func<Func<CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy
            ) : base(executionPolicy, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }
    }
}