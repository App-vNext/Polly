using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// Represents the options used to configure a retry strategy.
/// </summary>
public class RetryStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryStrategyOptions"/> class.
    /// </summary>
    public RetryStrategyOptions() => StrategyType = RetryConstants.StrategyType;

    /// <summary>
    /// Value that represents infinite retries.
    /// </summary>
    public const int InfiniteRetryCount = RetryConstants.InfiniteRetryCount;

    /// <summary>
    /// Gets or sets the maximum number of retries to use, in addition to the original call.
    /// </summary>
    /// <remarks>
    /// Defaults to 3 retries. For infinite retries use <see cref="InfiniteRetryCount"/> (-1).
    /// </remarks>
    [Range(InfiniteRetryCount, RetryConstants.MaxRetryCount)]
    public int RetryCount { get; set; } = RetryConstants.DefaultRetryCount;

    /// <summary>
    /// Gets or sets the type of the back-off.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="RetryBackoffType.ExponentialWithJitter"/>.
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
    /// By default, the predicate is empty and no results or exceptions are retried.
    /// </remarks>
    [Required]
    public Func<Outcome, ShouldRetryArguments, ValueTask<bool>>? ShouldRetry { get; set; }

    /// <summary>
    /// Gets or sets the generator instance that is used to calculate the time between retries.
    /// </summary>
    /// <remarks>
    /// By default, the generator is empty and it does not affect the delay between retries.
    /// </remarks>
    public Func<Outcome, RetryDelayArguments, ValueTask<TimeSpan>>? RetryDelayGenerator { get; set; }

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
    /// </remarks>
    public Func<Outcome, OnRetryArguments, ValueTask>? OnRetry { get; set; }
}
