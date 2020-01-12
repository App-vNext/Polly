using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    /// <summary>
    /// A wrapper for composing policies that can be applied to asynchronous executions.
    /// </summary>
    public partial class AsyncPolicyWrap : AsyncPolicy, IAsyncPolicyWrap
    {
        private readonly IAsyncPolicy _outer;
        private readonly IAsyncPolicy _inner;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Outer => _outer;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>
        /// </summary>
        public IsPolicy Inner => _inner;


        internal AsyncPolicyWrap(IAsyncPolicy outer, IAsyncPolicy inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal)
        {
            _outer = outer;
            _inner = inner;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task AsyncNonGenericImplementation(in IAsyncExecutable action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncPolicyWrapEngine.ImplementationAsync(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                _outer,
                _inner
            );

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncPolicyWrapEngine.ImplementationAsync<TExecutableAsync, TResult>(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                _outer,
                _inner
            );
        }
    }

    /// <summary>
    /// A wrapper for composing policies that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public partial class AsyncPolicyWrap<TResult> : AsyncPolicy<TResult>, IAsyncPolicyWrap<TResult>
    {
        private readonly IAsyncPolicy _outerNonGeneric;
        private readonly IAsyncPolicy _innerNonGeneric;

        private readonly IAsyncPolicy<TResult> _outerGeneric;
        private readonly IAsyncPolicy<TResult> _innerGeneric;

        /// <summary>
        /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Outer => (IsPolicy)_outerGeneric ?? _outerNonGeneric;

        /// <summary>
        /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>
        /// </summary>
        public IsPolicy Inner => (IsPolicy)_innerGeneric ?? _innerNonGeneric;

        internal AsyncPolicyWrap(IAsyncPolicy outer, IAsyncPolicy<TResult> inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal, ResultPredicates<TResult>.None)
        {
            _outerNonGeneric = outer;
            _innerGeneric = inner;
        }

        internal AsyncPolicyWrap(IAsyncPolicy<TResult> outer, IAsyncPolicy inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal, ((IResultPredicates<TResult>)outer).PredicatesInternal)
        {
            _outerGeneric = outer;
            _innerNonGeneric = inner;
        }

        internal AsyncPolicyWrap(IAsyncPolicy<TResult> outer, IAsyncPolicy<TResult> inner)
            : base(((IExceptionPredicates)outer).PredicatesInternal, ((IResultPredicates<TResult>)outer).PredicatesInternal)
        {
            _outerGeneric = outer;
            _innerGeneric = inner;
        }

        /// <inheritdoc/>
        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_outerNonGeneric != null)
            {
                if (_innerNonGeneric != null)
                {
                    return AsyncPolicyWrapEngine.ImplementationAsync<TExecutableAsync, TResult>(
                        action,
                        context,
                        cancellationToken,
                        continueOnCapturedContext,
                        _outerNonGeneric,
                        _innerNonGeneric
                    );
                }
                else if (_innerGeneric != null)
                {
                    return AsyncPolicyWrapEngine.ImplementationAsync<TExecutableAsync, TResult>(
                        action,
                        context,
                        cancellationToken,
                        continueOnCapturedContext,
                        _outerNonGeneric,
                        _innerGeneric
                    );

                }
                else
                {
                    throw new InvalidOperationException($"A {nameof(AsyncPolicyWrap<TResult>)} must define an inner policy.");
                }
            }
            else if (_outerGeneric != null)
            {
                if (_innerNonGeneric != null)
                {
                    return AsyncPolicyWrapEngine.ImplementationAsync<TExecutableAsync, TResult>(
                        action,
                        context,
                        cancellationToken,
                        continueOnCapturedContext,
                        _outerGeneric,
                        _innerNonGeneric
                    );

                }
                else if (_innerGeneric != null)
                {
                    return AsyncPolicyWrapEngine.ImplementationAsync<TExecutableAsync, TResult>(
                        action,
                        context,
                        cancellationToken,
                        continueOnCapturedContext,
                        _outerGeneric,
                        _innerGeneric
                    );

                }
                else
                {
                    throw new InvalidOperationException($"A {nameof(AsyncPolicyWrap<TResult>)} must define an inner policy.");
                }
            }
            else
            {
                throw new InvalidOperationException($"A {nameof(AsyncPolicyWrap<TResult>)} must define an outer policy.");
            }
        }
    }
}
