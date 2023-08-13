namespace Polly.Simmy;

/// <summary>
/// Defines the arguments for the <see cref="MonkeyStrategyOptions{TResult}.EnabledGenerator"/>.
/// </summary>
public sealed class EnabledGeneratorArguments
{
    /// <summary>
    /// Gets or sets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext? Context { get; set; }
}
