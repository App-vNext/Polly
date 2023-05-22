using Polly.Hedging;
using Polly.Strategy;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public const string Failure = "failure";

    public static ResilienceStrategy<string> CreateHedging()
    {
        return CreateStrategy(builder =>
        {
            builder.AddHedging(new HedgingStrategyOptions<string>
            {
                ShouldHandle = new OutcomePredicate<HandleHedgingArguments, string>().HandleResult(Failure),
                HedgingActionGenerator = args => () => Task.FromResult("hedged response"),
            });
        });
    }
}
