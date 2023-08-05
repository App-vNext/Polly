using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Behavior;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// Represents the options for the Behavior chaos strategy.
/// </summary>
public class BehaviorStrategyOptions : MonkeyStrategyOptions
{
    /// <summary>
    /// Gets or sets the delegate that's raised when the custom behavior is injected.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; set; }

    /// <summary>
    /// Gets or sets the custom behavior that is going to be injected for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<ResilienceContext, ValueTask> Behavior { get; set; }
}
