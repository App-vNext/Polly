// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Polly;

public readonly struct Outcome<TResult>
{
    public Exception? Exception { get; }
    public TResult? Result { get; }
    public void EnsureSuccess();
    public override string ToString();
}
