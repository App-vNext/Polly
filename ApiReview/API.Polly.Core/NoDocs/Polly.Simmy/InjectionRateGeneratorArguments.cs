// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy;

public readonly struct InjectionRateGeneratorArguments
{
    public ResilienceContext Context { get; }
    public InjectionRateGeneratorArguments(ResilienceContext context);
}
