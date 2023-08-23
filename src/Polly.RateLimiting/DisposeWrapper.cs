using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

internal sealed class DisposeWrapper : IDisposable, IAsyncDisposable
{
    internal DisposeWrapper(RateLimiter limiter) => Limiter = limiter;

    public RateLimiter Limiter { get; }

    public ValueTask DisposeAsync() => Limiter.DisposeAsync();

    public void Dispose() => Limiter.Dispose();
}
