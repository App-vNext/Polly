using System.ComponentModel.DataAnnotations;
using Polly.Hedging;
using Polly.Testing;

namespace Polly.Core.Tests.Hedging;

public class HedgingResiliencePipelineBuilderExtensionsTests
{
    private readonly ResiliencePipelineBuilder<string> _builder = new();

    [Fact]
    public void AddHedging_Generic_Ok()
    {
        _builder.AddHedging(new HedgingStrategyOptions<string>
        {
            ActionGenerator = args => () => Outcome.FromResultAsValueTask("dummy"),
            ShouldHandle = _ => PredicateResult.True()
        });

        _builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance
            .ShouldBeOfType<HedgingResilienceStrategy<string>>()
            .HedgingHandler.ActionGenerator.ShouldNotBeNull();
    }

    [Fact]
    public void AddHedgingT_InvalidOptions_Throws() =>
        Should.Throw<ValidationException>(() => _builder.AddHedging(new HedgingStrategyOptions<string> { ShouldHandle = null! }));

    [Fact]
    public async Task AddHedging_IntegrationTest()
    {
        int hedgingCount = 0;

        var strategy = _builder.AddHedging(new()
        {
            MaxHedgedAttempts = 4,
            Delay = System.Threading.Timeout.InfiniteTimeSpan,
            ShouldHandle = args => args.Outcome.Result switch
            {
                "error" => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            ActionGenerator = args =>
            {
                return () => args.AttemptNumber switch
                {
                    3 => Outcome.FromResultAsValueTask("success"),
                    _ => Outcome.FromResultAsValueTask("error")
                };
            },
            OnHedging = args =>
            {
                Interlocked.Increment(ref hedgingCount);
                return default;
            }
        })
        .Build();

        var result = await strategy.ExecuteAsync(token => new ValueTask<string>("error"));
        result.ShouldBe("success");
        hedgingCount.ShouldBe(3);
    }

    [Fact]
    public async Task AddHedging_IntegrationTestWithRealDelay()
    {
        var strategy = _builder.AddHedging(new()
        {
            MaxHedgedAttempts = 4,
            ShouldHandle = args => args.Outcome.Result switch
            {
                "error" => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            ActionGenerator = args =>
            {
                return async () =>
                {
                    await Task.Delay(20);

                    return args.AttemptNumber switch
                    {
                        3 => Outcome.FromResult("success"),
                        _ => Outcome.FromResult("error")
                    };
                };
            }
        })
        .Build();

        var result = await strategy.ExecuteAsync(token => new ValueTask<string>("error"));
        result.ShouldBe("success");
    }
}
