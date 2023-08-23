// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly;

public readonly struct ResiliencePropertyKey<TValue>
{
    public string Key { get; }
    public ResiliencePropertyKey(string key);
    public override string ToString();
}
