using Polly.Strategy;

namespace Polly.Timeout;

#pragma warning disable CA1815 // Equals not overridden because this class is just a data holder.

/// <summary>
/// Arguments used by the timeout strategy to retrieve a timeout for current execution.
/// </summary>
public readonly struct TimeoutGeneratorArguments : IResilienceArguments
{
    internal TimeoutGeneratorArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
