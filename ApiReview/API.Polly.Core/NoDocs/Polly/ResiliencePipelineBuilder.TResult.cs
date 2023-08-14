// Assembly 'Polly.Core'

namespace Polly;

public sealed class ResiliencePipelineBuilder<TResult> : ResiliencePipelineBuilderBase
{
    public ResiliencePipelineBuilder();
    public ResiliencePipeline<TResult> Build();
}
