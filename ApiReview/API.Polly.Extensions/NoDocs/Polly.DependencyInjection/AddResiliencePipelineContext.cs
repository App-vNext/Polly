// Assembly 'Polly.Extensions'

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Polly.Registry;

namespace Polly.DependencyInjection;

public sealed class AddResiliencePipelineContext<TKey> where TKey : notnull
{
    public TKey PipelineKey { get; }
    public IServiceProvider ServiceProvider { get; }
    public void EnableReloads<TOptions>(string? name = null);
    public TOptions GetOptions<TOptions>(string? name = null);
    public void OnPipelineDisposed(Action callback);
}
