// Assembly 'Polly.Testing'

using System.Collections.Generic;

namespace Polly.Testing;

public static class ResiliencePipelineExtensions
{
    public static ResiliencePipelineDescriptor GetPipelineDescriptor<TResult>(this ResiliencePipeline<TResult> pipeline);
    public static ResiliencePipelineDescriptor GetPipelineDescriptor(this ResiliencePipeline pipeline);
}