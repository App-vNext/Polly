using Polly.Hedging;
using Polly.Strategy;

namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public const string Failure = "failure";

    public static ResilienceStrategy<string> CreateHedging()
    {
        return CreateStrategy(builder =>
        {
            builder.AddHedging(new HedgingStrategyOptions<string>
            {
                ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Result == Failure),
                HedgingActionGenerator = args => () => Task.FromResult("hedged response"),
            });
        });
    }
}
