// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy.Outcomes;

public readonly struct FaultGeneratorArguments
{
    public ResilienceContext Context { get; }
    public FaultGeneratorArguments(ResilienceContext context);
}
