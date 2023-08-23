// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

public abstract class ResiliencePipelineProvider<TKey> where TKey : notnull
{
    public virtual ResiliencePipeline GetPipeline(TKey key);
    public virtual ResiliencePipeline<TResult> GetPipeline<TResult>(TKey key);
    public abstract bool TryGetPipeline(TKey key, [NotNullWhen(true)] out ResiliencePipeline? pipeline);
    public abstract bool TryGetPipeline<TResult>(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? pipeline);
    protected ResiliencePipelineProvider();
}
