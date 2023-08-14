// Assembly 'Polly.Core'

namespace Polly;

public sealed class NullResiliencePipeline<TResult> : ResiliencePipeline<TResult>
{
    public static readonly NullResiliencePipeline<TResult> Instance;
}
