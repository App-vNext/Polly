using System.ComponentModel.DataAnnotations;
using Polly.Builder;

namespace Polly.Timeout;

/// <summary>
/// The options for the timeout strategy.
/// </summary>
public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutStrategyOptions"/> class.
    /// </summary>
    public TimeoutStrategyOptions() => StrategyType = TimeoutConstants.StrategyType;

    /// <summary>
    /// Gets or sets the timeout generator that generates the timeout for a given execution.
    /// </summary>
    /// <remarks>
    /// By default, the generator provides <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> which effectively disables the timeout strategy.
    /// </remarks>
    [Required]
    public TimeoutGenerator TimeoutGenerator { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout event that notifies the timeout occurred.
    /// </summary>
    [Required]
    public OnTimeoutEvent OnTimeout { get; set; } = new();
}
