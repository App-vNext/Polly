using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// The options associated with the <see cref="MonkeyStrategyOptions"/>.
/// </summary>
public abstract class MonkeyStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the lambda to get injection rate between [0, 1].
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </remarks>
    [Required]
    [Range(MonkeyStrategyConstants.MinInjectionThreshold, MonkeyStrategyConstants.MaxInjectionThreshold)]
    public Func<ResilienceContext, ValueTask<double>> InjectionRate { get; set; }

    /// <summary>
    /// Gets or sets tge lambda to check if this policy is enabled in current context.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </remarks>
    [Required]
    public Func<ResilienceContext, ValueTask<bool>> Enabled { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="RandomUtil"/> instance.
    /// </summary>
    [Required]
    internal RandomUtil RandomUtil { get; set; }
}
