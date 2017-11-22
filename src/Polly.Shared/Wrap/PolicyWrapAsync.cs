using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Wrap
{
    public partial class PolicyWrap : IPolicyWrap
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> policyAction, Policy outer, IAsyncPolicy inner)
            : base(policyAction, outer.ExceptionPredicates)
        {
            _outer = outer;
            _inner = inner;
        }

        /// <summary>
        /// Executes the specified action asynchronously within the cache policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Execution context that is passed to the exception policy; defines the cache key to use in cache lookup.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action, or the cache.</returns>
        public override Task<TResult> ExecuteAsyncInternal<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return PolicyWrapEngine.ImplementationAsync<TResult>(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                (IAsyncPolicy)_outer,
                (IAsyncPolicy)_inner);
        }
    }

    public partial class PolicyWrap<TResult> : IPolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, Policy outer, IsPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
            Outer = outer;
            Inner = inner;
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction, Policy<TResult> outer, IsPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
            Outer = outer;
            Inner = inner;
        }
    }
}
