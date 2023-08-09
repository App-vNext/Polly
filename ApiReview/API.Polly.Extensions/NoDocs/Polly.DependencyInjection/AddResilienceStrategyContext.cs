// Assembly 'Polly.Extensions'

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Polly.Registry;

namespace Polly.DependencyInjection;

public sealed class AddResilienceStrategyContext<TKey> where TKey : notnull
{
    public string BuilderName { get; }
    public TKey StrategyKey { get; }
    public IServiceProvider ServiceProvider { get; }
    public void EnableReloads<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(string? name = null);
    public TOptions GetOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(string? name = null);
}
