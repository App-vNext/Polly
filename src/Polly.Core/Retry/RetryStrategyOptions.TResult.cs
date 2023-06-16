using System.ComponentModel.DataAnnotations;

namespace Polly.Retry;

/// <summary>
/// Represents the options used to configure a retry strategy.
/// </summary>
/// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
public class RetryStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>Retry</c> value.</remarks>
    public sealed override string StrategyType => RetryConstants.StrategyType;

    /// <summary>
    /// Gets or sets the maximum number of retries to use, in addition to the original call.
    /// </summary>
    /// <remarks>
    /// Defaults to 3 retries. For infinite retries use <see cref="RetryStrategyOptions.InfiniteRetryCount"/> (-1).
    /// </remarks>
    [Range(RetryStrategyOptions.InfiniteRetryCount, RetryConstants.MaxRetryCount)]
    public int RetryCount { get; set; } = RetryConstants.DefaultRetryCount;

    /// <summary>
    /// Gets or sets the type of the back-off.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="RetryBackoffType.Constant"/>.
    /// </remarks>
    public RetryBackoffType BackoffType { get; set; } = RetryConstants.DefaultBackoffType;

    /// <summary>
    /// Gets or sets the base delay between retries.
    /// </summary>
    /// <remarks>
    /// This value is used with the combination of <see cref="BackoffType"/> to generate the final delay for each individual retry attempt:
    /// <list type="bullet">
    /// <item>
    /// <see cref="RetryBackoffType.Exponential"/>: Represents the median delay to target before the first retry.
    /// </item>
    /// <item>
    /// <see cref="RetryBackoffType.ExponentialWithJitter"/>: Represents the median delay to target before the first retry.
    /// </item>
    /// <item>
    /// <see cref="RetryBackoffType.Linear"/>: Represents the initial delay, the following delays increasing linearly with this value.
    /// </item>
    /// <item>
    /// <see cref="RetryBackoffType.Constant"/> Represents the constant delay between retries.
    /// </item>
    /// </list>
    /// <para>
    /// Defaults to 2 seconds.
    /// </para>
    /// </remarks>
    [TimeSpan("00:00:00", "1.00:00:00")]
    public TimeSpan BaseDelay { get; set; } = RetryConstants.DefaultBaseDelay;

    /// <summary>
    /// Gets or sets an outcome predicate that is used to register the predicates to determine if a retry should be performed.
    /// </summary>
    /// <remarks>
    /// Defaults to a delegate that retries on any exception except <see cref="OperationCanceledException"/>.
    /// This property is required.
    /// </remarks>
    [Required]
    public Func<OutcomeArguments<TResult, ShouldRetryArguments>, ValueTask<bool>> ShouldRetry { get; set; } = args => args.Exception switch
    {
        OperationCanceledException => PredicateResult.False,
        Exception => PredicateResult.True,
        _ => PredicateResult.False
    };

    /// <summary>
    /// Gets or sets the generator instance that is used to calculate the time between retries.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OutcomeArguments<TResult, RetryDelayArguments>, ValueTask<TimeSpan>>? RetryDelayGenerator { get; set; }

    /// <summary>
    /// Gets or sets an outcome event that is used to register on-retry callbacks.
    /// </summary>
    /// <remarks>
    /// By default, the event is empty and no callbacks are registered.
    /// <para>
    /// After this event, the result produced the by user-callback is discarded and disposed to prevent resource over-consumption. If
    /// you need to preserve the result for further processing, create the copy of the result or extract and store all necessary information
    /// from the result within the event.
    /// </para>
    /// <para>
    /// Defaults to <see langword="null"/>.
    /// </para>
    /// </remarks>
    public Func<OutcomeArguments<TResult, OnRetryArguments>, ValueTask>? OnRetry { get; set; }
}
