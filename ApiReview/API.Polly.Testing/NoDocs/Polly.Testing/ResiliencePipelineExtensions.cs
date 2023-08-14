// Assembly 'Polly.Testing'

using System.Collections.Generic;

namespace Polly.Testing;

public static class ResiliencePipelineExtensions
{
    public static ResiliencePipelineDescriptor GetPipelineDescriptor<TResult>(this ResiliencePipeline<TResult> strategy);
    public static ResiliencePipelineDescriptor GetPipelineDescriptor(this ResiliencePipeline strategy);
}
