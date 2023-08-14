// Assembly 'Polly.Core'

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public sealed class ResiliencePipelineRegistry<TKey> : ResiliencePipelineProvider<TKey> where TKey : notnull
{
    public ResiliencePipelineRegistry();
    public ResiliencePipelineRegistry(ResiliencePipelineRegistryOptions<TKey> options);
    public bool TryAddPipeline(TKey key, ResiliencePipeline pipeline);
    public bool TryAddPipeline<TResult>(TKey key, ResiliencePipeline<TResult> pipeline);
    public bool RemovePipeline(TKey key);
    public bool RemovePipeline<TResult>(TKey key);
    public override bool TryGetPipeline<TResult>(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? pipeline);
    public override bool TryGetPipeline(TKey key, [NotNullWhen(true)] out ResiliencePipeline? pipeline);
    public ResiliencePipeline GetOrAddPipeline(TKey key, Action<ResiliencePipelineBuilder> configure);
    public ResiliencePipeline GetOrAddPipeline(TKey key, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>> configure);
    public ResiliencePipeline<TResult> GetOrAddPipeline<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>> configure);
    public ResiliencePipeline<TResult> GetOrAddPipeline<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure);
    public bool TryAddBuilder(TKey key, Action<ResiliencePipelineBuilder, ConfigureBuilderContext<TKey>> configure);
    public bool TryAddBuilder<TResult>(TKey key, Action<ResiliencePipelineBuilder<TResult>, ConfigureBuilderContext<TKey>> configure);
    public bool RemoveBuilder(TKey key);
    public bool RemoveBuilder<TResult>(TKey key);
    public void ClearPipelines();
    public void ClearPipelines<TResult>();
}
