using System.ComponentModel.DataAnnotations;

namespace Polly.Retry;

/// <summary>
/// Represents the options used to configure a retry strategy.
/// </summary>
/// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
public class RetryStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retries to use, in addition to the original call.
    /// </summary>
    /// <value>
    /// The default value is 3 retries.
    /// </value>
    [Range(1, RetryConstants.MaxRetryCount)]
    public int RetryCount { get; set; } = RetryConstants.DefaultRetryCount;

    /// <summary>
    /// Gets or sets the type of the back-off.
    /// </summary>
    /// <remarks>
    /// This property is ignored when <see cref="RetryDelayGenerator"/> is set.
    /// </remarks>
    /// <value>
    /// The default value is <see cref="RetryBackoffType.Constant"/>.
    /// </value>
    public RetryBackoffType BackoffType { get; set; } = RetryConstants.DefaultBackoffType;

    /// <summary>
    /// Gets or sets a value indicating whether jitter should be used when calculating the backoff delay between retries.
    /// </summary>
    /// <remarks>
    /// See <see href="https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation"/> for more details
    /// on how jitter can improve the resilience when the retries are correlated.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool UseJitter { get; set; }

#pragma warning disable IL2026 // Addressed with DynamicDependency on ValidationHelper.Validate method
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
    /// <see cref="RetryBackoffType.Linear"/>: Represents the initial delay, the following delays increasing linearly with this value.
    /// </item>
    /// <item>
    /// <see cref="RetryBackoffType.Constant"/> Represents the constant delay between retries.
    /// </item>
    /// </list>
    /// This property is ignored when <see cref="RetryDelayGenerator"/> is set.
    /// </remarks>
    /// <value>
    /// The default value is 2 seconds.
    /// </value>
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    public TimeSpan BaseDelay { get; set; } = RetryConstants.DefaultBaseDelay;
#pragma warning restore IL2026

    /// <summary>
    /// Gets or sets an outcome predicate that is used to register the predicates to determine if a retry should be performed.
    /// </summary>
    /// <value>
    /// The default is a delegate that retries on any exception except <see cref="OperationCanceledException"/>. This property is required.
    /// </value>
    [Required]
    public Func<OutcomeArguments<TResult, RetryPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; } = DefaultPredicates<RetryPredicateArguments, TResult>.HandleOutcome;

    /// <summary>
    /// Gets or sets the generator instance that is used to calculate the time between retries.
    /// </summary>
    /// <remarks>
    /// The generator has precedence over <see cref="BaseDelay"/> and <see cref="BackoffType"/>.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<OutcomeArguments<TResult, RetryDelayArguments>, ValueTask<TimeSpan>>? RetryDelayGenerator { get; set; }

    /// <summary>
    /// Gets or sets an outcome event that is used to register on-retry callbacks.
    /// </summary>
    /// <remarks>
    /// After this event, the result produced the by user-callback is discarded and disposed to prevent resource over-consumption. If
    /// you need to preserve the result for further processing, create the copy of the result or extract and store all necessary information
    /// from the result within the event.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<OutcomeArguments<TResult, OnRetryArguments>, ValueTask>? OnRetry { get; set; }
}
