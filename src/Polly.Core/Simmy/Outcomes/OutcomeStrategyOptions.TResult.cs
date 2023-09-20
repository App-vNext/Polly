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
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="Outcome"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="Outcome"/> is used.
    /// </remarks>
    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>>? OutcomeGenerator { get; set; }

    /// <summary>
    /// Gets or sets the outcome to be injected for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="OutcomeGenerator"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="OutcomeGenerator"/> is used.
    /// </remarks>
    public Outcome<TResult>? Outcome { get; set; }
}
