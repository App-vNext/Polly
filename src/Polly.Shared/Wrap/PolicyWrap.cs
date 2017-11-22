using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Wrap
{
    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    public partial class PolicyWrap : Policy, IPolicyWrap
    {
        private IsPolicy _outer;
        private IsPolicy _inner;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Outer => _outer;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Inner => _inner;

        internal PolicyWrap(Action<Action<Context, CancellationToken>, Context, CancellationToken> policyAction, Policy outer, ISyncPolicy inner) 
            : base(policyAction, outer.ExceptionPredicates)
        {
            _outer = outer;
            _inner = inner;
        }

        /// <summary>
        /// Executes the specified action within the cache policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Execution context that is passed to the exception policy; defines the cache key to use in cache lookup.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action, or the cache.</returns>
        public override TResult ExecuteInternal<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return PolicyWrapEngine.Implementation<TResult>(
                   action,
                   context,
                   cancellationToken,
                   (ISyncPolicy)_outer,
                   (ISyncPolicy)_inner
                   );
        }
    }

    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public partial class PolicyWrap<TResult> : Policy<TResult>, IPolicyWrap<TResult>
    {
        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Outer { get; private set; }

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Inner { get; private set; }

        internal PolicyWrap(Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction, Policy outer, IsPolicy inner)
            : base(policyAction, outer.ExceptionPredicates,  PredicateHelper<TResult>.EmptyResultPredicates)
        {
            Outer = outer;
            Inner = inner;
        }

        internal PolicyWrap(Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> policyAction, Policy<TResult> outer, IsPolicy inner)
            : base(policyAction, outer.ExceptionPredicates, outer.ResultPredicates)
        {
            Outer = outer;
            Inner = inner;
        }
    }
}
