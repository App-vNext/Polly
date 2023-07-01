using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// The options associated with the <see cref="MonkeyStrategyOptions"/>.
/// </summary>
public abstract class MonkeyStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the injection rate for a given execution, which the value should be between [0, 1].
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="InjectionRateGenerator"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="InjectionRateGenerator"/> is used.
    /// </remarks>
    [Range(MonkeyStrategyConstants.MinInjectionThreshold, MonkeyStrategyConstants.MaxInjectionThreshold)]
    public double? InjectionRate { get; set; }

    /// <summary>
    /// Gets or sets the injection rate generator for a given execution, which the value should be between [0, 1].
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="InjectionRate"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="InjectionRate"/> is used.
    /// </remarks>
    public Func<ResilienceContext, ValueTask<double>> InjectionRateGenerator { get; set; }

    /// <summary>
    /// Gets or sets the enable generator that indicates whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="Enabled"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="Enabled"/> is used.
    /// </remarks>
    public Func<ResilienceContext, ValueTask<bool>> EnabledGenerator { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="EnabledGenerator"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="EnabledGenerator"/> is used.
    /// </remarks>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Randomizer generator instance that is used to evaluate the injection rate.
    /// </summary>
    /// <remarks>
    /// The default randomizer is thread safe and returns values between 0.0 and 1.0.
    /// </remarks>
    [Required]
    public Func<double> Randomizer { get; set; } = RandomUtil.Instance.NextDouble;
}

