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
    public int MaxRetryAttempts { get; set; }
    public DelayBackoffType BackoffType { get; set; }
    public bool UseJitter { get; set; }
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    public TimeSpan Delay { get; set; }
    [Required]
    public Func<RetryPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; }
    public Func<RetryDelayGeneratorArguments<TResult>, ValueTask<TimeSpan>>? DelayGenerator { get; set; }
    public Func<OnRetryArguments<TResult>, ValueTask>? OnRetry { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Required]
    public Func<double> Randomizer { get; set; }
    public RetryStrategyOptions();
}
