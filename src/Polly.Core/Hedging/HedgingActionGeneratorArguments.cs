using Polly.Strategy;

namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in <see cref="HedgingActionGenerator"/>.
/// </summary>
public readonly struct HedgingActionGeneratorArguments : IResilienceArguments
{
    internal HedgingActionGeneratorArguments(ResilienceContext context, int attempt)
    {
        Context = context;
        Attempt = attempt;
    }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the zero-based hedging attempt number.
    /// </summary>
    public int Attempt { get; }
}
