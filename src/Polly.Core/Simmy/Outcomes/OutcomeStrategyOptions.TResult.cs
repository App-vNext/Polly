using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Outcomes;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// Represents the options for the Outcome chaos strategy.
/// </summary>
/// <typeparam name="TResult">The type of the outcome that was injected.</typeparam>
internal class OutcomeStrategyOptions<TResult> : MonkeyStrategyOptions
{
    /// <summary>
    /// Gets or sets the delegate that's raised when the outcome is injected.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnOutcomeInjectedArguments<TResult>, ValueTask>? OnOutcomeInjected { get; set; }

    /// <summary>
    /// Gets or sets the outcome generator to be injected for a given execution.
    /// </summary>
    [Required]
    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>> OutcomeGenerator { get; set; }
}
