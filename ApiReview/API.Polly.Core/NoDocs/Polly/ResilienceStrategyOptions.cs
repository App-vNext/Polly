// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly;

public abstract class ResilienceStrategyOptions
{
    public string? StrategyName { get; set; }
    public abstract string StrategyType { get; }
    protected ResilienceStrategyOptions();
}
