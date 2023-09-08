// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy;

public readonly struct EnabledGeneratorArguments
{
    public ResilienceContext Context { get; }
    public EnabledGeneratorArguments(ResilienceContext context);
}
