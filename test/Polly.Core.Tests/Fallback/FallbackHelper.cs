using Polly.Fallback;

namespace Polly.Core.Tests.Fallback;

internal static class FallbackHelper
{
    public static FallbackHandler<T> CreateHandler<T>(
        Func<Outcome<T>, bool> shouldHandle,
        Func<Outcome<T>> fallback) =>
        new(
            args => new ValueTask<bool>(shouldHandle(args.Outcome)),
            _ => new ValueTask<Outcome<T>>(fallback()));
}

