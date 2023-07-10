// Assembly 'Polly.Extensions'

using System;
using System.Runtime.CompilerServices;
using Polly.Registry;

namespace Polly.Extensions.DependencyInjection;

public sealed class AddResilienceStrategyContext<TKey> where TKey : notnull
{
    public string BuilderName { get; }
    public TKey StrategyKey { get; }
    public IServiceProvider ServiceProvider { get; }
    public void EnableReloads<TOptions>(string? name = null);
    public TOptions GetOptions<TOptions>(string? name = null);
}
