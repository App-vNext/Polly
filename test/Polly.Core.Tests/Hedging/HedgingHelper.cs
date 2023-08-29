using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly.Core.Tests.Hedging;

internal static class HedgingHelper
{
    public static HedgingHandler<T> CreateHandler<T>(
        Func<Outcome<T>, bool> shouldHandle,
        Func<HedgingActionGeneratorArguments<T>, Func<ValueTask<Outcome<T>>>?> generator)
    {
        return new HedgingHandler<T>(args => new ValueTask<bool>(shouldHandle(args.Outcome!))!, generator);
    }
}

