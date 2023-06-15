// Assembly 'Polly.Core'

namespace Polly.Hedging;

public readonly record struct HedgingDelayArguments(ResilienceContext Context, int Attempt);
