// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Hedging;

public readonly struct HedgingDelayGeneratorArguments
{
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public HedgingDelayGeneratorArguments(ResilienceContext context, int attemptNumber);
}
