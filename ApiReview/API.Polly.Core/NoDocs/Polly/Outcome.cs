// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly;

public static class Outcome
{
    public static Outcome<TResult> FromResult<TResult>(TResult? value);
    public static ValueTask<Outcome<TResult>> FromResultAsTask<TResult>(TResult value);
    public static Outcome<TResult> FromException<TResult>(Exception exception);
    public static ValueTask<Outcome<TResult>> FromExceptionAsTask<TResult>(Exception exception);
}
