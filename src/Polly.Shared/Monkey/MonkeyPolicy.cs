using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Monkey
{
    /// <summary>
    /// A Monkey policy that can be applied to delegates.
    /// </summary>
    public partial class MonkeyPolicy : Policy, IMonkeyPolicy
    {
        internal MonkeyPolicy(Action<Action<Context, CancellationToken>, Context, CancellationToken> exceptionPolicy)
            : base(exceptionPolicy, PredicateHelper.EmptyExceptionPredicates)
        {
        }
    }

    /// <summary>
    /// A Monkey policy that can be applied to delegates returning a value of type <typeparamref name="TResult" />
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    public partial class MonkeyPolicy<TResult> : Policy<TResult>, IMonkeyPolicy<TResult>
    {
        internal MonkeyPolicy(
             Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy
             ) : base(executionPolicy, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
        }
    }
}
