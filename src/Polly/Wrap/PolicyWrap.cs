using System;
using System.Diagnostics;
using System.Threading;

namespace Polly.Wrap
{
    /// <summary>
    /// A wrapper for composing policies that can be applied to synchronous executions.
    /// </summary>
    public partial class PolicyWrap : Policy, ISyncPolicyWrap
    {
        private readonly ISyncPolicy _outer;
        private readonly ISyncPolicy _inner;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Outer => _outer;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Inner => _inner;

        internal PolicyWrap(ISyncPolicy outer, ISyncPolicy inner) 
            : base(((IExceptionPredicates)outer).PredicatesInternal)
        {
            _outer = outer;
            _inner = inner;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override void SyncNonGenericImplementation(in ISyncExecutable action, Context context, CancellationToken cancellationToken)
            => PolicyWrapEngine.Implementation(
                action,
                context,
                cancellationToken,
                _outer,
                _inner
            );

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementation<TExecutable, TResult>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
            => PolicyWrapEngine.Implementation<TExecutable, TResult>(
                action,
                context,
                cancellationToken,
                _outer,
                _inner
            );
    }

    /// <summary>
    /// A wrapper for composing policies that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public partial class PolicyWrap<TResult> : Policy<TResult>, ISyncPolicyWrap<TResult>
    {
        private readonly ISyncPolicy _outerNonGeneric;
        private readonly ISyncPolicy _innerNonGeneric;

        private readonly ISyncPolicy<TResult> _outerGeneric;
        private readonly ISyncPolicy<TResult> _innerGeneric;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Outer => (IsPolicy)_outerGeneric ?? _outerNonGeneric;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Inner => (IsPolicy)_innerGeneric ?? _innerNonGeneric;

        internal PolicyWrap(ISyncPolicy outer, ISyncPolicy<TResult> inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal, ResultPredicates<TResult>.None)
        {
            _outerNonGeneric = outer;
            _innerGeneric = inner;
        }

        internal PolicyWrap(ISyncPolicy<TResult> outer, ISyncPolicy inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal, ((IResultPredicates<TResult>)outer).PredicatesInternal)
        {
            _outerGeneric = outer;
            _innerNonGeneric = inner;
        }

        internal PolicyWrap(ISyncPolicy<TResult> outer, ISyncPolicy<TResult> inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal, ((IResultPredicates<TResult>)outer).PredicatesInternal)
        {
            _outerGeneric = outer;
            _innerGeneric = inner;
        }

        /// <inheritdoc/>
        protected override TResult SyncGenericImplementation<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (_outerNonGeneric != null)
            {
                if (_innerNonGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TExecutable, TResult>(
                        action,
                        context,
                        cancellationToken,
                        _outerNonGeneric,
                        _innerNonGeneric
                    );
                }
                else if (_innerGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TExecutable, TResult>(
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
                    return PolicyWrapEngine.Implementation<TExecutable, TResult>(
                        action,
                        context,
                        cancellationToken,
                        _outerGeneric,
                        _innerNonGeneric
                    );

                }
                else if (_innerGeneric != null)
                {
                    return PolicyWrapEngine.Implementation<TExecutable, TResult>(
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
