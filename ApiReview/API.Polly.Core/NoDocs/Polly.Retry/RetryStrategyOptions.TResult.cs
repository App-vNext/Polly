// Assembly 'Polly.Core'

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Retry;

public class RetryStrategyOptions<TResult> : ResilienceStrategyOptions
{
    [Range(1, int.MaxValue)]
    public int RetryCount { get; set; }
    public RetryBackoffType BackoffType { get; set; }
    public bool UseJitter { get; set; }
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    public TimeSpan BaseDelay { get; set; }
    [Required]
    public Func<OutcomeArguments<TResult, RetryPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; }
    public Func<OutcomeArguments<TResult, RetryDelayArguments>, ValueTask<TimeSpan>>? RetryDelayGenerator { get; set; }
    public Func<OutcomeArguments<TResult, OnRetryArguments>, ValueTask>? OnRetry { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Required]
    public Func<double> Randomizer { get; set; }
    public RetryStrategyOptions();
}
