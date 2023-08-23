// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly;

public abstract class ResilienceStrategyOptions
{
    public string? Name { get; set; }
    protected ResilienceStrategyOptions();
}
