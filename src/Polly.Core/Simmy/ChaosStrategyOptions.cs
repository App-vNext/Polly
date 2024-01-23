using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy;

/// <summary>
/// The options associated with the <see cref="ChaosStrategy"/>.
/// </summary>
public abstract class ChaosStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the injection rate for a given execution, which the value should be between [0, 1] (inclusive).
    /// </summary>
    /// <remarks>
    /// Defaults to 0.001, meaning one in a thousand executions/0.1%. Either <see cref="InjectionRateGenerator"/> or this property is required.
    /// </remarks>
    [Range(ChaosStrategyConstants.MinInjectionThreshold, ChaosStrategyConstants.MaxInjectionThreshold)]
    public double InjectionRate { get; set; } = ChaosStrategyConstants.DefaultInjectionRate;

    /// <summary>
    /// Gets or sets the injection rate generator for a given execution, which the value should be between [0, 1] (inclusive).
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="InjectionRate"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="InjectionRate"/> is used.
    /// </remarks>
    public Func<InjectionRateGeneratorArguments, ValueTask<double>>? InjectionRateGenerator { get; set; }

    /// <summary>
    /// Gets or sets the enable generator that indicates whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="Enabled"/> or this property is required.
    /// When this property is <see langword="null"/> then the <see cref="Enabled"/> property is used.
    /// </remarks>
    public Func<EnabledGeneratorArguments, ValueTask<bool>>? EnabledGenerator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true"/>. Either <see cref="EnabledGenerator"/> or this property is required.
    /// </remarks>
    public bool Enabled { get; set; } = ChaosStrategyConstants.DefaultEnabled;

    /// <summary>
    /// Gets or sets the Randomizer generator instance that is used to evaluate the injection rate.
    /// </summary>
    /// <remarks>
    /// The default randomizer is thread safe and returns values between 0.0 and 1.0.
    /// </remarks>
    [Required]
    public Func<double> Randomizer { get; set; } = RandomUtil.Instance.NextDouble;
}
