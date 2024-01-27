using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Outcomes;

/// <summary>
/// Represents the options for the outcome chaos strategy.
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
    /// <value>
    /// Defaults to <see langword="null"/>.
    /// </value>
    public Func<OnOutcomeInjectedArguments<TResult>, ValueTask>? OnOutcomeInjected { get; set; }

    /// <summary>
    /// Gets or sets the generator that generates the outcomes to be injected.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </value>
    [Required]
    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>> OutcomeGenerator { get; set; } = default!;
}
