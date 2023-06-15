// Assembly 'Polly.RateLimiting'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Polly.RateLimiting;

public class RateLimiterStrategyOptions : ResilienceStrategyOptions
{
    public sealed override string StrategyType { get; }
    public Func<OnRateLimiterRejectedArguments, ValueTask>? OnRejected { get; set; }
    [Required]
    public RateLimiter? RateLimiter { get; set; }
    public RateLimiterStrategyOptions();
}
