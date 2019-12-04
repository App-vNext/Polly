using System;
using System.Diagnostics;
using System.Threading;

namespace Polly.Wrap
{
    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    public partial class PolicyWrap : Policy, IPolicyWrap
    {
        private ISyncPolicy _outer;
        private ISyncPolicy _inner;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Outer => _outer;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Inner => _inner;

        internal PolicyWrap(Policy outer, ISyncPolicy inner) 
            : base(outer.ExceptionPredicates)
        {
            _outer = outer;
            _inner = inner;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
            => PolicyWrapEngine.Implementation(
                action,
                context,
                cancellationToken,
                _outer,
                _inner
            );

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            =>  PolicyWrapEngine.Implementation<TResult>(
                action,
                context,
                cancellationToken,
                _outer,
                _inner
            );
    }

    /// <summary>
    /// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public partial class PolicyWrap<TResult> : Policy<TResult>, IPolicyWrap<TResult>
    {
        private ISyncPolicy _outerNonGeneric;
        private ISyncPolicy _innerNonGeneric;

        private ISyncPolicy<TResult> _outerGeneric;
        private ISyncPolicy<TResult> _innerGeneric;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Outer => (IsPolicy)_outerGeneric ?? _outerNonGeneric;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Inner => (IsPolicy)_innerGeneric ?? _innerNonGeneric;

        internal PolicyWrap(Policy outer, ISyncPolicy<TResult> inner)
            : base(outer.ExceptionPredicates, ResultPredicates<TResult>.None)
        {
            _outerNonGeneric = outer;
            _innerGeneric = inner;
        }

        internal PolicyWrap(Policy<TResult> outer, ISyncPolicy inner)
            : base(outer.ExceptionPredicates, outer.ResultPredicates)
        {
            _outerGeneric = outer;
            _innerNonGeneric = inner;
        }

        internal PolicyWrap(Policy<TResult> outer, ISyncPolicy<TResult> inner)
            : base(outer.ExceptionPredicates, outer.ResultPredicates)
        {
            _outerGeneric = outer;
            _innerGeneric = inner;
        }

        /// <inheritdoc/>
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_outerNonGeneric != null)
            {
                if (_innerNonGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TResult>(
                        action,
                        context,
                        cancellationToken,
                        _outerNonGeneric,
                        _innerNonGeneric
                    );
                }
                else if (_innerGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TResult>(
                        action,
                        context,
                        cancellationToken,
                        _outerNonGeneric,
                        _innerGeneric
                    );

                }
                else
                {
                    throw new InvalidOperationException($"A {nameof(PolicyWrap<TResult>)} must define an inner policy.");
                }
            }
            else if (_outerGeneric != null)
            {
                if (_innerNonGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TResult>(
                        action,
                        context,
                        cancellationToken,
                        _outerGeneric,
                        _innerNonGeneric
                    );

                }
                else if (_innerGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TResult>(
                        action,
                        context,
                        cancellationToken,
                        _outerGeneric,
                        _innerGeneric
                    );

                }
                else
                {
                    throw new InvalidOperationException($"A {nameof(PolicyWrap<TResult>)} must define an inner policy.");
                }
            }
            else
            {
                throw new InvalidOperationException($"A {nameof(PolicyWrap<TResult>)} must define an outer policy.");
            }
        }
    }
}
