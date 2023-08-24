using System.ComponentModel;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Retry;

namespace Polly;

#pragma warning disable CA2225 // Operator overloads have named alternates

public partial class PredicateBuilder<TResult>
{
    /// <summary>
    /// The operator that converts <paramref name="builder"/> to <see cref="RetryStrategyOptions{TResult}.ShouldHandle"/> delegate.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<RetryPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder)
    {
        Guard.NotNull(builder);
        return builder.Build<RetryPredicateArguments<TResult>>();
    }

    /// <summary>
    /// The operator that converts <paramref name="builder"/> to <see cref="RetryStrategyOptions{TResult}.ShouldHandle"/> delegate.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<HedgingPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder)
    {
        Guard.NotNull(builder);
        return builder.Build<HedgingPredicateArguments<TResult>>();
    }

    /// <summary>
    /// The operator that converts <paramref name="builder"/> to <see cref="RetryStrategyOptions{TResult}.ShouldHandle"/> delegate.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<FallbackPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder)
    {
        Guard.NotNull(builder);
        return builder.Build<FallbackPredicateArguments<TResult>>();
    }

    /// <summary>
    /// The operator that converts <paramref name="builder"/> to <see cref="RetryStrategyOptions{TResult}.ShouldHandle"/> delegate.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder)
    {
        Guard.NotNull(builder);
        return builder.Build<CircuitBreakerPredicateArguments<TResult>>();
    }
}
