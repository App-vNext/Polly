using Polly.Fallback;

namespace Polly.Core.Tests.Fallback;

internal static class FallbackHelper
{
    public static FallbackHandler<T> CreateHandler<T>(
        Func<Outcome<T>, bool> shouldHandle,
        Func<Outcome<T>> fallback,
        bool isGeneric = true)
    {
        return new FallbackHandler<T>(
            args => new ValueTask<bool>(shouldHandle(args.Outcome)),
            _ => fallback().AsValueTask(),
            isGeneric);
    }
}

