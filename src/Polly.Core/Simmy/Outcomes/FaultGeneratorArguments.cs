namespace Polly.Simmy.Outcomes;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the fault chaos strategy to ge the fault that is going to be injected.
/// </summary>
public readonly struct FaultGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FaultGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    public FaultGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
