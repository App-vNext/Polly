using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using NSubstitute;
using Polly.Testing;

namespace Polly.RateLimiting.Tests;

public class RateLimiterCompositeStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<CompositeStrategyBuilder<int>>> Data = new()
    {
        builder =>
        {
            builder.AddConcurrencyLimiter(2, 2);
            AssertRateLimiterStrategy(builder);
        },
        builder =>
        {
            builder.AddConcurrencyLimiter(
                new ConcurrencyLimiterOptions
                {
                    PermitLimit = 2,
                    QueueLimit = 2
                });

            AssertRateLimiterStrategy(builder);
        },
        builder =>
        {
            var expected = Substitute.For<RateLimiter>();
            builder.AddRateLimiter(expected);
            AssertRateLimiterStrategy(builder);
        }
    };

    [MemberData(nameof(Data))]
    [Theory(Skip = "https://github.com/stryker-mutator/stryker-net/issues/2144")]
    public void AddRateLimiter_Extensions_Ok(Action<CompositeStrategyBuilder<int>> configure)
    {
        var builder = new CompositeStrategyBuilder<int>();

        configure(builder);

        builder.Build().Should().BeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddConcurrencyLimiter_InvalidOptions_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            return new CompositeStrategyBuilder().AddConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = -10,
                QueueLimit = -10
            })
            .Build();
        });
    }

    [Fact]
    public void AddRateLimiter_AllExtensions_Ok()
    {
        foreach (var configure in Data.Select(v => v[0]).Cast<Action<CompositeStrategyBuilder<int>>>())
        {
            var builder = new CompositeStrategyBuilder<int>();

            configure(builder);

            builder.Build().GetInnerStrategies().Strategies.Single().StrategyType.Should().Be(typeof(RateLimiterResilienceStrategy));
        }
    }

    [Fact]
    public void AddRateLimiter_Ok()
    {
        using var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            QueueLimit = 10,
            PermitLimit = 10
        });

        new CompositeStrategyBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = ResilienceRateLimiter.Create(limiter)
            })
            .Build()
            .GetInnerStrategies().Strategies.Single()
            .StrategyType
            .Should()
            .Be<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddRateLimiter_InvalidOptions_Throws()
    {
        new CompositeStrategyBuilder().Invoking(b => b.AddRateLimiter(new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The 'RateLimiterStrategyOptions' are invalid.

            Validation Errors:
            The DefaultRateLimiterOptions field is required.
            """);
    }

    [Fact]
    public void AddGenericRateLimiter_InvalidOptions_Throws()
    {
        new CompositeStrategyBuilder<int>().Invoking(b => b.AddRateLimiter(new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The 'RateLimiterStrategyOptions' are invalid.

            Validation Errors:
            The DefaultRateLimiterOptions field is required.
            """);
    }

    [Fact]
    public void AddRateLimiter_Options_Ok()
    {
        var strategy = new CompositeStrategyBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = ResilienceRateLimiter.Create(Substitute.For<RateLimiter>())
            })
            .Build()
            .GetInnerStrategies().Strategies
            .Single()
            .StrategyType
            .Should()
            .Be<RateLimiterResilienceStrategy>();
    }

    private static void AssertRateLimiterStrategy(CompositeStrategyBuilder<int> builder)
    {
        builder.Build().GetInnerStrategies().Strategies.Single().StrategyType.Should().Be(typeof(RateLimiterResilienceStrategy));
    }
}
