using System.ComponentModel.DataAnnotations;
using Polly.Hedging;
using Polly.Testing;

namespace Polly.Core.Tests.Hedging;

public class HedgingResiliencePipelineBuilderExtensionsTests
{
    private readonly ResiliencePipelineBuilder _builder = new();
    private readonly ResiliencePipelineBuilder<string> _genericBuilder = new();

    [Fact]
    public void AddHedging_Ok()
    {
        _builder.AddHedging(new HedgingStrategyOptions { ShouldHandle = _ => PredicateResult.True });

        _builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance
            .Should().BeOfType<HedgingResilienceStrategy<object>>().Subject
            .HedgingHandler.IsGeneric.Should().BeFalse();
    }

    [Fact]
    public void AddHedging_Generic_Ok()
    {
        _genericBuilder.AddHedging(new HedgingStrategyOptions<string>
        {
            HedgingActionGenerator = args => () => Outcome.FromResultAsTask("dummy"),
            ShouldHandle = _ => PredicateResult.True
        });

        _genericBuilder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance
            .Should().BeOfType<HedgingResilienceStrategy<string>>().Subject
            .HedgingHandler.IsGeneric.Should().BeTrue();
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
                ShouldHandle = args => args.Outcome.Result switch
                {
                    "error" => PredicateResult.True,
                    _ => PredicateResult.False
                },
                HedgingActionGenerator = args =>
                {
                    return async () =>
                    {
                        await Task.Delay(25, args.ActionContext.CancellationToken);

                        if (args.AttemptNumber == 3)
                        {
                            return Outcome.FromResult((object)"success");
                        }

                        return Outcome.FromResult((object)"error");
                    };
                },
                OnHedging = args =>
                {
                    if (args.Outcome is { } outcome)
                    {
                        results.Enqueue(outcome.Result!.ToString()!);
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
