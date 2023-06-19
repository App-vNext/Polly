using Polly.Fallback;
using Polly.Utils;

namespace Polly.Core.Tests.Fallback;

internal static class FallbackHelper
{
    public static FallbackHandler<T> CreateHandler<T>(
        Func<Outcome<T>, bool> shouldHandle,
        Func<Outcome<T>> fallback,
        bool isGeneric = true)
    {
        return new FallbackHandler<T>(
            PredicateInvoker<FallbackPredicateArguments>.Create<T>(args => new ValueTask<bool>(shouldHandle(args.Outcome!)), true)!,
            _ => fallback().AsValueTask(),
            isGeneric);
    }
}

