#nullable enable
using System;

namespace Polly.Retry.Options;

public abstract class SleepDurationProviderBase : IDisposable
{
    public abstract TimeSpan GetNext(int iteration, Exception exception, Context context);

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public abstract class SleepDurationProviderBase<TResult> : IDisposable
{
    public abstract TimeSpan GetNext(int iteration, DelegateResult<TResult> result, Context context);

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}