using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Fault;

/// <summary>
/// Represents the options for the fault chaos strategy.
/// </summary>
public class ChaosFaultStrategyOptions : ChaosStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosFaultStrategyOptions"/> class.
    /// </summary>
    public ChaosFaultStrategyOptions() => Name = ChaosFaultConstants.DefaultName;

    /// <summary>
    /// Gets or sets the delegate that's raised when the fault is injected.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>.
    /// </value>
    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; set; } = default!;

    /// <summary>
    /// Gets or sets the fault generator to be used for fault injection.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </value>
    [Required]
    public Func<FaultGeneratorArguments, ValueTask<Exception?>>? FaultGenerator { get; set; } = default!;
}
