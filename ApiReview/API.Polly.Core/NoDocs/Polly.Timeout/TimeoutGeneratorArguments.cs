// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Timeout;

public readonly struct TimeoutGeneratorArguments
{
    public ResilienceContext Context { get; }
    public TimeoutGeneratorArguments(ResilienceContext context);
}
