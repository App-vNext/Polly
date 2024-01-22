using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Fault;

/// <summary>
/// Represents the options for the Fault chaos strategy.
/// </summary>
public class FaultStrategyOptions : ChaosStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FaultStrategyOptions"/> class.
    /// </summary>
    public FaultStrategyOptions() => Name = FaultConstants.DefaultName;

    /// <summary>
    /// Gets or sets the delegate that's raised when the outcome is injected.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; set; } = default!;

    /// <summary>
    /// Gets or sets the fault generator to be injected for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<FaultGeneratorArguments, ValueTask<Exception?>>? FaultGenerator { get; set; } = default!;
}
