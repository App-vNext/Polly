namespace Polly.Simmy.Behavior;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the behavior chaos strategy to notify that a custom behavior was injected.
/// </summary>
internal readonly struct OnBehaviorInjectedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnBehaviorInjectedArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    public OnBehaviorInjectedArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
