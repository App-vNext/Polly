using System;
using System.Threading.Tasks;

namespace Polly.Utils;

#pragma warning disable CA1031 // Do not catch general exception types

internal static class DisposeHelper
{
    public static async ValueTask TryDisposeSafeAsync<T>(T value)
    {
        try
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
        catch (Exception e)
        {
            Debug.Assert(false, e.ToString());
        }
    }
}
