using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Fault;

/// <summary>
/// Represents the options for the Fault chaos strategy.
/// </summary>
public class FaultStrategyOptions : MonkeyStrategyOptions
{
    /// <summary>
    /// Gets or sets the delegate that's raised when the outcome is injected.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; set; } = default!;

    /// <summary>
    /// Gets or sets the outcome generator to be injected for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="Fault"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="Fault"/> is used.
    /// </remarks>
    [Required]
    public Func<FaultGeneratorArguments, ValueTask<Exception?>>? FaultGenerator { get; set; } = default!;
}
