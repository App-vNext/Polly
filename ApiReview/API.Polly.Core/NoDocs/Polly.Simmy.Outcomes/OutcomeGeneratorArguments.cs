// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy.Outcomes;

public readonly struct OutcomeGeneratorArguments
{
    public ResilienceContext Context { get; }
    public OutcomeGeneratorArguments(ResilienceContext context);
}
