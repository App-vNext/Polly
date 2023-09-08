// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy.Behavior;

public readonly struct OnBehaviorInjectedArguments
{
    public ResilienceContext Context { get; }
    public OnBehaviorInjectedArguments(ResilienceContext context);
}
