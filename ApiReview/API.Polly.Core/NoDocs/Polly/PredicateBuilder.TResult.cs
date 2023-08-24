// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Retry;
using Polly.Utils;

namespace Polly;

public class PredicateBuilder<TResult>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<RetryPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<HedgingPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<FallbackPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>>(PredicateBuilder<TResult> builder);
    public PredicateBuilder<TResult> Handle<TException>() where TException : Exception;
    public PredicateBuilder<TResult> Handle<TException>(Func<TException, bool> predicate) where TException : Exception;
    public PredicateBuilder<TResult> HandleInner<TException>() where TException : Exception;
    public PredicateBuilder<TResult> HandleInner<TException>(Func<TException, bool> predicate) where TException : Exception;
    public PredicateBuilder<TResult> HandleResult(Func<TResult, bool> predicate);
    public PredicateBuilder<TResult> HandleResult(TResult result, IEqualityComparer<TResult>? comparer = null);
    public Predicate<Outcome<TResult>> Build();
    public PredicateBuilder();
}
