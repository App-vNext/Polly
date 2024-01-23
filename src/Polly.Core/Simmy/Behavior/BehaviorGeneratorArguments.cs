namespace Polly.Simmy.Behavior;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the behavior chaos strategy to execute a user's delegate custom action.
/// </summary>
public readonly struct BehaviorGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BehaviorGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    public BehaviorGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
