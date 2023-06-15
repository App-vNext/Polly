// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly;

public sealed class NullResilienceStrategy : ResilienceStrategy
{
    public static readonly NullResilienceStrategy Instance;
    protected internal override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state);
}
