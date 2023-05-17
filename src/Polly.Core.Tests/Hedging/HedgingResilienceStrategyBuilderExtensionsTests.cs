using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyBuilderExtensionsTests
{
    private readonly ResilienceStrategyBuilder _builder = new();

    public static readonly TheoryData<Action<ResilienceStrategyBuilder>> HedgingCases = new()
    {
        builder =>
        {
            builder.AddHedging(new HedgingStrategyOptions());
        },
        builder =>
        {
            builder.AddHedging(new HedgingStrategyOptions<double>
            {
                HedgingActionGenerator = args => () => Task.FromResult<double>(0)
            });
        },
    };

    [MemberData(nameof(HedgingCases))]
    [Theory]
    public void AddHedging_Ok(Action<ResilienceStrategyBuilder> configure)
    {
        configure(_builder);
        _builder.Build().Should().BeOfType<HedgingResilienceStrategy>();
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
            .Invoking(b => b.AddHedging(new HedgingStrategyOptions<double> { ShouldHandle = null! }))
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
                    handler.ShouldHandle.HandleResult("error");
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
                OnHedging = new Polly.Strategy.OutcomeEvent<OnHedgingArguments>().Register<string>((outcome, _) => results.Enqueue(outcome.Result!))
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
