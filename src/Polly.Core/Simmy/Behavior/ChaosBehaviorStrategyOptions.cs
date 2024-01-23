using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Behavior;

/// <summary>
/// Represents the options for the Behavior chaos strategy.
/// </summary>
public class ChaosBehaviorStrategyOptions : ChaosStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosBehaviorStrategyOptions"/> class.
    /// </summary>
    public ChaosBehaviorStrategyOptions() => Name = ChaosBehaviorConstants.DefaultName;

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
    public Func<BehaviorGeneratorArguments, ValueTask>? BehaviorGenerator { get; set; }
}
