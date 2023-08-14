// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly;

public abstract class ResilienceStrategy
{
    protected internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state);
    protected ResilienceStrategy();
}
