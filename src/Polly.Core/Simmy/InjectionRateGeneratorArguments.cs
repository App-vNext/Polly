namespace Polly.Simmy;

/// <summary>
/// Defines the arguments for the <see cref="MonkeyStrategyOptions{TResult}.InjectionRateGenerator"/>.
/// </summary>
public sealed class InjectionRateGeneratorArguments
{
    /// <summary>
    /// Gets or sets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext? Context { get; set; }
}
