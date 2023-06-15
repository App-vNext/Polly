// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Polly;

public class ResilienceStrategy<TResult>
{
    public ValueTask<TResult> ExecuteAsync<TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state);
    public ValueTask<TResult> ExecuteAsync(Func<ResilienceContext, ValueTask<TResult>> callback, ResilienceContext context);
    public ValueTask<TResult> ExecuteAsync<TState>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken = default(CancellationToken));
    public ValueTask<TResult> ExecuteAsync(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default(CancellationToken));
    public ValueTask<Outcome<TResult>> ExecuteOutcomeAsync<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state);
    public TResult Execute<TState>(Func<ResilienceContext, TState, TResult> callback, ResilienceContext context, TState state);
    public TResult Execute(Func<ResilienceContext, TResult> callback, ResilienceContext context);
    public TResult Execute(Func<CancellationToken, TResult> callback, CancellationToken cancellationToken = default(CancellationToken));
    public TResult Execute(Func<TResult> callback);
    public TResult Execute<TState>(Func<TState, TResult> callback, TState state);
    public TResult Execute<TState>(Func<TState, CancellationToken, TResult> callback, TState state, CancellationToken cancellationToken = default(CancellationToken));
}
