namespace Polly.Wrap;

/// <summary>
/// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
/// </summary>
public partial class PolicyWrap : Policy, IPolicyWrap
{
    private readonly ISyncPolicy _outer;
    private readonly ISyncPolicy _inner;

    /// <summary>
    /// Gets the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>.
    /// </summary>
    public IsPolicy Outer => _outer;

    /// <summary>
    /// Gets the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>.
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
    protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken) =>
        PolicyWrapEngine.Implementation(
            action,
            context,
            _outer,
            _inner, cancellationToken);

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        PolicyWrapEngine.Implementation<TResult>(
            action,
            context,
            _outer,
            _inner, cancellationToken);
}

/// <summary>
/// A policy that allows two (and by recursion more) Polly policies to wrap executions of delegates.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
public partial class PolicyWrap<TResult> : Policy<TResult>, IPolicyWrap<TResult>
{
    private readonly ISyncPolicy _outerNonGeneric;
    private readonly ISyncPolicy _innerNonGeneric;

    private readonly ISyncPolicy<TResult> _outerGeneric;
    private readonly ISyncPolicy<TResult> _innerGeneric;

    /// <summary>
    /// Gets the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>.
    /// </summary>
    public IsPolicy Outer => (IsPolicy)_outerGeneric ?? _outerNonGeneric;

    /// <summary>
    /// Gets the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap{TResult}"/>.
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
                    _outerNonGeneric,
                    _innerNonGeneric, cancellationToken);
            }
            else if (_innerGeneric != null)
            {
                return PolicyWrapEngine.Implementation<TResult>(
                    action,
                    context,
                    _outerNonGeneric,
                    _innerGeneric, cancellationToken);
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
                    _outerGeneric,
                    _innerNonGeneric, cancellationToken);

            }
            else if (_innerGeneric != null)
            {
                return PolicyWrapEngine.Implementation<TResult>(
                    action,
                    context,
                    _outerGeneric,
                    _innerGeneric, cancellationToken);
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
