// Assembly 'Polly.RateLimiting'

using System.Runtime.CompilerServices;

namespace Polly.RateLimiting;

public readonly struct RateLimiterArguments
{
    public ResilienceContext Context { get; }
    public RateLimiterArguments(ResilienceContext context);
}
