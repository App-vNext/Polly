using Polly.Hedging;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public const string Failure = "failure";

    public static ResilienceStrategy CreateHedging()
    {
        return CreateStrategy(builder =>
        {
            builder.AddHedging(new HedgingStrategyOptions
            {
                Handler = new HedgingHandler().SetHedging<string>(handler =>
                {
                    handler.ShouldHandle.HandleResult(Failure);
                    handler.HedgingActionGenerator = args => () => Task.FromResult("hedged response");
                })
            });
        });
    }
}
