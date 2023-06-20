using System.ComponentModel.DataAnnotations;
using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyBuilderExtensionsTests
{
    private readonly ResilienceStrategyBuilder _builder = new();
    private readonly ResilienceStrategyBuilder<string> _genericBuilder = new();

    [Fact]
    public void AddHedging_Ok()
    {
        _builder.AddHedging(new HedgingStrategyOptions { ShouldHandle = _ => PredicateResult.True });
        _builder.Build().Should().BeOfType<HedgingResilienceStrategy<object>>();
    }

    [Fact]
    public void AddHedging_Generic_Ok()
    {
        _genericBuilder.AddHedging(new HedgingStrategyOptions<string>
        {
            HedgingActionGenerator = args => () => Outcome.FromResultAsTask("dummy"),
            ShouldHandle = _ => PredicateResult.True
        });
        _genericBuilder.Build().Strategy.Should().BeOfType<HedgingResilienceStrategy<string>>();
    }

    [Fact]
    public void AddHedging_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions { HedgingActionGenerator = null! }))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddHedgingT_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions { MaxHedgedAttempts = 1000 }))
            .Should()
            .Throw<ValidationException>();

        _genericBuilder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions<string> { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public async Task AddHedging_IntegrationTest()
    {
        var hedgingWithoutOutcome = false;
        ConcurrentQueue<string> results = new();

        var strategy = _builder
            .AddHedging(new HedgingStrategyOptions
            {
                MaxHedgedAttempts = 4,
                HedgingDelay = TimeSpan.FromMilliseconds(20),
                ShouldHandle = args => args.Result switch
                {
                    "error" => PredicateResult.True,
                    _ => PredicateResult.False
                },
                HedgingActionGenerator = args =>
                {
                    return async () =>
                    {
                        await Task.Delay(25, args.ActionContext.CancellationToken);

                        if (args.Attempt == 3)
                        {
                            return Outcome.FromResult((object)"success");
                        }

                        return Outcome.FromResult((object)"error");
                    };
                },
                OnHedging = args =>
                {
                    if (args.Arguments.HasOutcome)
                    {
                        results.Enqueue(args.Result!.ToString()!);
                    }
                    else
                    {
                        hedgingWithoutOutcome = true;
                    }

                    return default;
                }
            })
            .Build();

        var result = await strategy.ExecuteAsync(async token =>
        {
            await Task.Delay(25, token);
            return "error";
        });

        result.Should().Be("success");
        results.Should().HaveCountGreaterThan(0);
        results.Distinct().Should().ContainSingle("error");
        hedgingWithoutOutcome.Should().BeTrue();
    }
}
