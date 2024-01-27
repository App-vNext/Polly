using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Behavior;

/// <summary>
/// Represents the options for the behavior chaos strategy.
/// </summary>
public class ChaosBehaviorStrategyOptions : ChaosStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosBehaviorStrategyOptions"/> class.
    /// </summary>
    public ChaosBehaviorStrategyOptions() => Name = ChaosBehaviorConstants.DefaultName;

    /// <summary>
    /// Gets or sets the delegate that's raised when the behavior is injected.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>.
    /// </value>
    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; set; }

    /// <summary>
    /// Gets or sets the behavior that is going to be injected for a given execution.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </value>
    [Required]
    public Func<BehaviorGeneratorArguments, ValueTask>? BehaviorGenerator { get; set; }
}
