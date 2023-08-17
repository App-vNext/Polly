// Assembly 'Polly.Core'

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Polly.Utils;

namespace Polly.Registry;

public sealed class ResiliencePipelineRegistry<TKey> : ResiliencePipelineProvider<TKey> where TKey : notnull
{
    public ResiliencePipelineRegistry();
    public ResiliencePipelineRegistry(ResiliencePipelineRegistryOptions<TKey> options);
    public override bool TryGetPipeline<TResult>(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? pipeline);
    public override bool TryGetPipeline(TKey key, [NotNullWhen(true)] out ResiliencePipeline? pipeline);
    public ResiliencePipeline GetOrAddPipeline(TKey key, Action<ResiliencePipelineBuilder> configure);
    public ResiliencePipeline GetOrAddPipeline(TKey key, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>> configure);
    public ResiliencePipeline<TResult> GetOrAddPipeline<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>> configure);
    public ResiliencePipeline<TResult> GetOrAddPipeline<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure);
    public bool TryAddBuilder(TKey key, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>> configure);
    public bool TryAddBuilder<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure);
}
