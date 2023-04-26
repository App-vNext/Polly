using Polly.Strategy;

namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by hedging delay generator.
/// </summary>
public readonly struct HedgingDelayArguments : IResilienceArguments
{
    internal HedgingDelayArguments(ResilienceContext context, int attempt)
    {
        Context = context;
        Attempt = attempt;
    }

    /// <summary>
    /// Gets the zero-based hedging attempt number.
    /// </summary>
    public int Attempt { get; }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
