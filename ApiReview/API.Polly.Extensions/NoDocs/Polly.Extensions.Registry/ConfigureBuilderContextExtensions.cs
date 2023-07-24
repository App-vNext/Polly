// Assembly 'Polly.Extensions'

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Polly.Extensions.Registry;

public static class ConfigureBuilderContextExtensions
{
    public static void EnableReloads<TKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(this ConfigureBuilderContext<TKey> context, IOptionsMonitor<TOptions> optionsMonitor, string? name = null) where TKey : notnull;
}
