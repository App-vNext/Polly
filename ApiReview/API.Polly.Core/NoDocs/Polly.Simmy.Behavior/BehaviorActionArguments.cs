// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy.Behavior;

public readonly struct BehaviorActionArguments
{
    public ResilienceContext Context { get; }
    public BehaviorActionArguments(ResilienceContext context);
}
