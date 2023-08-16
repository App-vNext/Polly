// Assembly 'Polly.Extensions'

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Polly.Registry;

public static class ConfigureBuilderContextExtensions
{
    public static void EnableReloads<TKey, TOptions>(this ConfigureBuilderContext<TKey> context, IOptionsMonitor<TOptions> optionsMonitor, string? name = null) where TKey : notnull;
}
