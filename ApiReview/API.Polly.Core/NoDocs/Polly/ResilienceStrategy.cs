// Assembly 'Polly.Core'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Polly;

public abstract class ResilienceStrategy
{
    public ValueTask ExecuteAsync<TState>(Func<ResilienceContext, TState, ValueTask> callback, ResilienceContext context, TState state);
    public ValueTask ExecuteAsync(Func<ResilienceContext, ValueTask> callback, ResilienceContext context);
    public ValueTask ExecuteAsync<TState>(Func<TState, CancellationToken, ValueTask> callback, TState state, CancellationToken cancellationToken = default(CancellationToken));
    public ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default(CancellationToken));
    public ValueTask<Outcome<TResult>> ExecuteOutcomeAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state);
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state);
    public ValueTask<TResult> ExecuteAsync<TResult>(Func<ResilienceContext, ValueTask<TResult>> callback, ResilienceContext context);
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken = default(CancellationToken));
    public ValueTask<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default(CancellationToken));
    protected internal abstract ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state);
    public void Execute<TState>(Action<ResilienceContext, TState> callback, ResilienceContext context, TState state);
    public void Execute(Action<ResilienceContext> callback, ResilienceContext context);
    public void Execute<TState>(Action<TState, CancellationToken> callback, TState state, CancellationToken cancellationToken = default(CancellationToken));
    public void Execute(Action<CancellationToken> callback, CancellationToken cancellationToken = default(CancellationToken));
    public void Execute<TState>(Action<TState> callback, TState state);
    public void Execute(Action callback);
    public TResult Execute<TResult, TState>(Func<ResilienceContext, TState, TResult> callback, ResilienceContext context, TState state);
    public TResult Execute<TResult>(Func<ResilienceContext, TResult> callback, ResilienceContext context);
    public TResult Execute<TResult>(Func<CancellationToken, TResult> callback, CancellationToken cancellationToken = default(CancellationToken));
    public TResult Execute<TResult>(Func<TResult> callback);
    public TResult Execute<TResult, TState>(Func<TState, TResult> callback, TState state);
    public TResult Execute<TResult, TState>(Func<TState, CancellationToken, TResult> callback, TState state, CancellationToken cancellationToken = default(CancellationToken));
    protected ResilienceStrategy();
}
