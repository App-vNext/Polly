// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Polly;

public readonly struct Outcome<TResult>
{
    public Exception? Exception { get; }
    public TResult? Result { get; }
    public bool HasResult { get; }
    public bool IsVoidResult { get; }
    public Outcome(Exception exception);
    public Outcome(TResult? result);
    public void EnsureSuccess();
    public bool TryGetResult(out TResult? result);
    public override string ToString();
}
