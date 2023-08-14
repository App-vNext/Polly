// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly;

public sealed class NullResiliencePipeline : ResiliencePipeline
{
    public static readonly NullResiliencePipeline Instance;
}
