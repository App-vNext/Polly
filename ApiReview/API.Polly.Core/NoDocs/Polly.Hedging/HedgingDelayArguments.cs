// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Hedging;

public readonly struct HedgingDelayArguments
{
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public HedgingDelayArguments(ResilienceContext context, int attemptNumber);
}
