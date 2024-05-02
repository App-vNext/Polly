namespace Polly.Wrap;

/// <summary>
/// A policy that allows two (and by recursion more) async Polly policies to wrap executions of async delegates.
/// </summary>
public partial class AsyncPolicyWrap : AsyncPolicy, IPolicyWrap
{
    private readonly IAsyncPolicy _outer;
    private readonly IAsyncPolicy _inner;

    /// <summary>
    /// Gets the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>.
    /// </summary>
    public IsPolicy Outer => _outer;

    /// <summary>
    /// Gets the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>.
    /// </summary>
    public IsPolicy Inner => _inner;

    internal AsyncPolicyWrap(AsyncPolicy outer, IAsyncPolicy inner)
        : base(outer.ExceptionPredicates)
    {
        _outer = outer;
        _inner = inner;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task ImplementationAsync(
        Func<Context, CancellationToken, Task> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncPolicyWrapEngine.ImplementationAsync(
            action,
            context,
            cancellationToken,
            continueOnCapturedContext,
            _outer,
            _inner);

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncPolicyWrapEngine.ImplementationAsync<TResult>(
            action,
            context,
            cancellationToken,
            continueOnCapturedContext,
            _outer,
            _inner);
}

/// <summary>
/// A policy that allows two (and by recursion more) async Polly policies to wrap executions of async delegates.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
public partial class AsyncPolicyWrap<TResult> : AsyncPolicy<TResult>, IPolicyWrap<TResult>
{
    private readonly IAsyncPolicy _outerNonGeneric;
    private readonly IAsyncPolicy _innerNonGeneric;

    private readonly IAsyncPolicy<TResult> _outerGeneric;
    private readonly IAsyncPolicy<TResult> _innerGeneric;

    /// <summary>
    /// Gets the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>.
    /// </summary>
    public IsPolicy Outer => (IsPolicy)_outerGeneric ?? _outerNonGeneric;

    /// <summary>
    /// Gets the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>.
    /// </summary>
    public IsPolicy Inner => (IsPolicy)_innerGeneric ?? _innerNonGeneric;

    internal AsyncPolicyWrap(AsyncPolicy outer, IAsyncPolicy<TResult> inner)
        : base(outer.ExceptionPredicates, ResultPredicates<TResult>.None)
    {
        _outerNonGeneric = outer;
        _innerGeneric = inner;
    }

    internal AsyncPolicyWrap(AsyncPolicy<TResult> outer, IAsyncPolicy inner)
        : base(outer.ExceptionPredicates, outer.ResultPredicates)
    {
        _outerGeneric = outer;
        _innerNonGeneric = inner;
    }

    internal AsyncPolicyWrap(AsyncPolicy<TResult> outer, IAsyncPolicy<TResult> inner)
        : base(outer.ExceptionPredicates, outer.ResultPredicates)
    {
        _outerGeneric = outer;
        _innerGeneric = inner;
    }

    /// <inheritdoc/>
    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        if (_outerNonGeneric != null)
        {
            if (_innerNonGeneric != null)
            {
                return AsyncPolicyWrapEngine.ImplementationAsync<TResult>(
                    action,
                    context,
                    cancellationToken,
                    continueOnCapturedContext,
                    _outerNonGeneric,
                    _innerNonGeneric);
            }
            else if (_innerGeneric != null)
            {
                return AsyncPolicyWrapEngine.ImplementationAsync<TResult>(
                    action,
                    context,
                    cancellationToken,
                    continueOnCapturedContext,
                    _outerNonGeneric,
                    _innerGeneric);

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
                return AsyncPolicyWrapEngine.ImplementationAsync<TResult>(
                    action,
                    context,
                    cancellationToken,
                    continueOnCapturedContext,
                    _outerGeneric,
                    _innerNonGeneric);

            }
            else if (_innerGeneric != null)
            {
                return AsyncPolicyWrapEngine.ImplementationAsync<TResult>(
                    action,
                    context,
                    cancellationToken,
                    continueOnCapturedContext,
                    _outerGeneric,
                    _innerGeneric);

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
