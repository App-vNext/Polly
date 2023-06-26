// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Retry;

namespace Polly;

public class PredicateBuilder<TResult>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<OutcomeArguments<TResult, RetryPredicateArguments>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<OutcomeArguments<TResult, HedgingPredicateArguments>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<OutcomeArguments<TResult, CircuitBreakerPredicateArguments>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    public PredicateBuilder<TResult> Handle<TException>() where TException : Exception;
    public PredicateBuilder<TResult> Handle<TException>(Func<TException, bool> predicate) where TException : Exception;
    public PredicateBuilder<TResult> HandleInner<TException>() where TException : Exception;
    public PredicateBuilder<TResult> HandleInner<TException>(Func<TException, bool> predicate) where TException : Exception;
    public PredicateBuilder<TResult> HandleResult(Func<TResult, bool> predicate);
    public PredicateBuilder<TResult> HandleResult(TResult result, IEqualityComparer<TResult>? comparer = null);
    public Predicate<Outcome<TResult>> Build();
    public Func<OutcomeArguments<TResult, TArgs>, ValueTask<bool>> Build<TArgs>();
    public PredicateBuilder();
}
