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
            .Should().BeOfType<HedgingResilienceStrategy<string>>().Subject
            .HedgingHandler.ActionGenerator.Should().NotBeNull();
    }

    [Fact]
    public void AddHedgingT_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions<string> { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public async Task AddHedging_IntegrationTest()
    {
        int hedgingCount = 0;

        var strategy = _builder
            .AddHedging(new()
            {
                MaxHedgedAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(20),
                ShouldHandle = args => args.Outcome.Result switch
                {
                    "error" => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                ActionGenerator = args =>
                {
                    return async () =>
                    {
                        await Task.Delay(25, args.ActionContext.CancellationToken);

                        if (args.AttemptNumber == 3)
                        {
                            return Outcome.FromResult("success");
                        }

                        return Outcome.FromResult("error");
                    };
                },
                OnHedging = args =>
                {
                    hedgingCount++;
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
        hedgingCount.Should().Be(4);
    }
}
