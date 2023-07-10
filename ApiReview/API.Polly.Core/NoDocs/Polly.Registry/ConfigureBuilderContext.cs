// Assembly 'Polly.Core'

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Polly.Registry;

public class ConfigureBuilderContext<TKey> where TKey : notnull
{
    public TKey StrategyKey { get; }
    public string BuilderName { get; }
    public string? BuilderInstanceName { get; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void EnableReloads(Func<Func<CancellationToken>> tokenProducerFactory);
}
