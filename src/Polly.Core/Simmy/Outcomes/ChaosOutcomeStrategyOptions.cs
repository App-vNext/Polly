using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Outcomes;

/// <summary>
/// Represents the options for the Outcome chaos strategy.
/// </summary>
/// <typeparam name="TResult">The type of the outcome that was injected.</typeparam>
public class ChaosOutcomeStrategyOptions<TResult> : ChaosStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosOutcomeStrategyOptions{TResult}"/> class.
    /// </summary>
    public ChaosOutcomeStrategyOptions() => Name = ChaosOutcomeConstants.DefaultName;

    /// <summary>
    /// Gets or sets the delegate that's invoked when the outcome is injected.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnOutcomeInjectedArguments<TResult>, ValueTask>? OnOutcomeInjected { get; set; }

    /// <summary>
    /// Gets or sets the outcome generator to be injected for a given execution.
    /// </summary>
    [Required]
    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>> OutcomeGenerator { get; set; } = default!;
}
