namespace Polly.Utils;

#pragma warning disable CA1031 // Do not catch general exception types

internal static class DisposeHelper
{
    public static async ValueTask TryDisposeSafeAsync<T>(T value, bool isSynchronous)
    {
        try
        {
            // for synchronous executions we want to prefer the synchronous dispose method
            if (isSynchronous)
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else if (value is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
            }
            else
            {
                if (value is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        catch (Exception)
        {
            // Swallow any exception
        }
    }
}
