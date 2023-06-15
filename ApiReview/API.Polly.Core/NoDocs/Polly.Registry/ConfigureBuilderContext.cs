// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Polly.Registry;

public class ConfigureBuilderContext<TKey> where TKey : notnull
{
    public TKey StrategyKey { get; }
    public string BuilderName { get; }
    public string StrategyKeyString { get; }
    public void EnableReloads(Func<CancellationToken> reloadTokenProducer);
}
