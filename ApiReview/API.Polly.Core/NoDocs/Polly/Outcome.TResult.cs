// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Polly;

public readonly struct Outcome<TResult>
{
    public Exception? Exception { get; }
    public TResult? Result { get; }
    public bool HasResult { get; }
    public bool IsVoidResult { get; }
    public void EnsureSuccess();
    public bool TryGetResult(out TResult? result);
    public override string ToString();
}
