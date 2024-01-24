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
    /// <value>
    /// Defaults to <c>0.001</c>, meaning one in a thousand executions/0.1%. When <see cref="InjectionRateGenerator"/> is specified, this property is ignored.
    /// </value>
    [Range(ChaosStrategyConstants.MinInjectionThreshold, ChaosStrategyConstants.MaxInjectionThreshold)]
    public double InjectionRate { get; set; } = ChaosStrategyConstants.DefaultInjectionRate;

    /// <summary>
    /// Gets or sets the injection rate generator for a given execution, which the value should be between [0, 1] (inclusive).
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>. When generator is specified, the <see cref="InjectionRate"/> property is ignored.
    /// </value>
    public Func<InjectionRateGeneratorArguments, ValueTask<double>>? InjectionRateGenerator { get; set; }

    /// <summary>
    /// Gets or sets the enable generator that indicates whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>. When the generator is specified, the <see cref="Enabled"/> property is ignored.
    /// </value>
    public Func<EnabledGeneratorArguments, ValueTask<bool>>? EnabledGenerator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="true"/>. When <see cref="EnabledGenerator"/> is specified, this property is ignored.
    /// </value>
    public bool Enabled { get; set; } = ChaosStrategyConstants.DefaultEnabled;

    /// <summary>
    /// Gets or sets the Randomizer generator instance that is used to evaluate the injection rate.
    /// </summary>
    /// <value>
    /// The default randomizer is thread safe and returns values between <c>0.0</c> and <c>1.0</c>.
    /// </value>
    [Required]
    public Func<double> Randomizer { get; set; } = RandomUtil.Instance.NextDouble;
}
