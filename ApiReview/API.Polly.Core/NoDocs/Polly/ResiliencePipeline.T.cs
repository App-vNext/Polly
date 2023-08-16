// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utils;

namespace Polly;

public sealed class ResiliencePipeline<T>
{
    public static readonly ResiliencePipeline<T> Null;
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state) where TResult : T;
    public ValueTask<TResult> ExecuteAsync<TResult>(Func<ResilienceContext, ValueTask<TResult>> callback, ResilienceContext context) where TResult : T;
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken = default(CancellationToken)) where TResult : T;
    public ValueTask<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default(CancellationToken)) where TResult : T;
    public ValueTask<Outcome<TResult>> ExecuteOutcomeAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state) where TResult : T;
    public TResult Execute<TResult, TState>(Func<ResilienceContext, TState, TResult> callback, ResilienceContext context, TState state) where TResult : T;
    public TResult Execute<TResult>(Func<ResilienceContext, TResult> callback, ResilienceContext context) where TResult : T;
    public TResult Execute<TResult>(Func<CancellationToken, TResult> callback, CancellationToken cancellationToken = default(CancellationToken)) where TResult : T;
    public TResult Execute<TResult>(Func<TResult> callback) where TResult : T;
    public TResult Execute<TResult, TState>(Func<TState, TResult> callback, TState state) where TResult : T;
    public TResult Execute<TResult, TState>(Func<TState, CancellationToken, TResult> callback, TState state, CancellationToken cancellationToken = default(CancellationToken)) where TResult : T;
}
