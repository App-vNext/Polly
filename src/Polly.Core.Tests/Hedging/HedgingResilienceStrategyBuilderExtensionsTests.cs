using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Polly.Hedging;
using Polly.Strategy;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyBuilderExtensionsTests
{
    private readonly ResilienceStrategyBuilder _builder = new();
    private readonly ResilienceStrategyBuilder<string> _genericBuilder = new();

    [Fact]
    public void AddHedging_Ok()
    {
        _builder.AddHedging(new HedgingStrategyOptions());
        _builder.Build().Should().BeOfType<HedgingResilienceStrategy>();
    }

    [Fact]
    public void AddHedging_Generic_Ok()
    {
        _genericBuilder.AddHedging(new HedgingStrategyOptions<string>
        {
            HedgingActionGenerator = args => () => Task.FromResult("dummy"),
            ShouldHandle = (_, _) => PredicateResult.True
        });
        _genericBuilder.Build().Strategy.Should().BeOfType<HedgingResilienceStrategy>();
    }

    [Fact]
    public void AddHedging_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions { Handler = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The hedging strategy options are invalid.*");
    }

    [Fact]
    public void AddHedgingT_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions { MaxHedgedAttempts = 1000 }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The hedging strategy options are invalid.*");

        _genericBuilder
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions<string> { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The hedging strategy options are invalid.*");
    }

    [Fact]
    public async Task AddHedging_IntegrationTest()
    {
        ConcurrentQueue<string> results = new();

        var strategy = _builder
            .AddHedging(new HedgingStrategyOptions
            {
                MaxHedgedAttempts = 4,
                HedgingDelay = TimeSpan.FromMilliseconds(20),
                Handler = new HedgingHandler().SetHedging<string>(handler =>
                {
                    handler.ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Result == "error");
                    handler.HedgingActionGenerator = args =>
                    {
                        return async () =>
                        {
                            await Task.Delay(25, args.Context.CancellationToken);

                            if (args.Attempt == 3)
                            {
                                return "success";
                            }

                            return "error";
                        };
                    };
                }),
                OnHedging = (outcome, _) => { results.Enqueue(outcome.Result!.ToString()!); return default; }
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
    }
}
