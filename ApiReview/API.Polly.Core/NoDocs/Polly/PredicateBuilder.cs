// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polly;

public sealed class PredicateBuilder<TResult>
{
    public PredicateBuilder<TResult> Handle<TException>() where TException : Exception;
    public PredicateBuilder<TResult> Handle<TException>(Func<TException, bool> predicate) where TException : Exception;
    public PredicateBuilder<TResult> HandleInner<TException>() where TException : Exception;
    public PredicateBuilder<TResult> HandleInner<TException>(Func<TException, bool> predicate) where TException : Exception;
    public PredicateBuilder<TResult> HandleResult(Func<TResult, bool> predicate);
    public PredicateBuilder<TResult> HandleResult(TResult result, IEqualityComparer<TResult>? comparer = null);
}
