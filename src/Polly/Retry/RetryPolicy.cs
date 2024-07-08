#nullable enable
namespace Polly.Retry;

/// <summary>
/// A retry policy that can be applied to synchronous delegates.
/// </summary>
public class RetryPolicy : Policy, IRetryPolicy
{
    private readonly Action<Exception, TimeSpan, int, Context> _onRetry;
    private readonly int _permittedRetryCount;
    private readonly IEnumerable<TimeSpan>? _sleepDurationsEnumerable;
    private readonly Func<int, Exception, Context, TimeSpan>? _sleepDurationProvider;

    internal RetryPolicy(
        PolicyBuilder policyBuilder,
        Action<Exception, TimeSpan, int, Context> onRetry,
        int permittedRetryCount = int.MaxValue,
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
        Func<int, Exception, Context, TimeSpan>? sleepDurationProvider = null)
        : base(policyBuilder)
    {
        _permittedRetryCount = permittedRetryCount;
        _sleepDurationsEnumerable = sleepDurationsEnumerable;
        _sleepDurationProvider = sleepDurationProvider;
        _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
    }

    /// <inheritdoc/>
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        var sleepDurationProvider = _sleepDurationProvider != null
            ? (retryCount, outcome, ctx) => _sleepDurationProvider(retryCount, outcome.Exception, ctx)
            : (Func<int, DelegateResult<TResult>, Context, TimeSpan>?)null;

        return RetryEngine.Implementation(action,
            context,
            ExceptionPredicates,
            ResultPredicates<TResult>.None,
            (outcome, timespan, retryCount, ctx) => _onRetry(outcome.Exception, timespan, retryCount, ctx),
            cancellationToken,
            _permittedRetryCount,
            _sleepDurationsEnumerable,
            sleepDurationProvider);
    }
}

/// <summary>
/// A retry policy that can be applied to synchronous delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class RetryPolicy<TResult> : Policy<TResult>, IRetryPolicy<TResult>
{
    private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;
    private readonly int _permittedRetryCount;
    private readonly IEnumerable<TimeSpan>? _sleepDurationsEnumerable;
    private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan>? _sleepDurationProvider;

    internal RetryPolicy(
        PolicyBuilder<TResult> policyBuilder,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry,
        int permittedRetryCount = int.MaxValue,
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
        Func<int, DelegateResult<TResult>, Context, TimeSpan>? sleepDurationProvider = null)
        : base(policyBuilder)
    {
        _permittedRetryCount = permittedRetryCount;
        _sleepDurationsEnumerable = sleepDurationsEnumerable;
        _sleepDurationProvider = sleepDurationProvider;
        _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        RetryEngine.Implementation(
            action,
            context,
            ExceptionPredicates,
            ResultPredicates,
            _onRetry,
            cancellationToken,
            _permittedRetryCount,
            _sleepDurationsEnumerable,
            _sleepDurationProvider);
}
