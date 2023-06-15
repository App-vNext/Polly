// Assembly 'Polly.Core'

namespace Polly;

public sealed class NullResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    public static readonly NullResilienceStrategy<TResult> Instance;
}
