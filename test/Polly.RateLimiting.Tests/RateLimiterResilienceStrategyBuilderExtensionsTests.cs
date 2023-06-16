using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Moq;

namespace Polly.RateLimiting.Tests;

public class RateLimiterResilienceStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResilienceStrategyBuilder<int>>> Data = new()
    {
        builder =>
        {
            builder.AddConcurrencyLimiter(2, 2);
            AssertConcurrencyLimiter(builder, hasEvents: false);
        },
        builder =>
        {
            builder.AddConcurrencyLimiter(
                new ConcurrencyLimiterOptions
                {
                    PermitLimit = 2,
                    QueueLimit = 2
                });

            AssertConcurrencyLimiter(builder, hasEvents: false);
        },
        builder =>
        {
            var expected = Mock.Of<RateLimiter>();
            builder.AddRateLimiter(expected);
            AssertRateLimiter(builder, hasEvents: false, limiter => limiter.Should().Be(expected));
        }
    };

    [MemberData(nameof(Data))]
    [Theory(Skip = "https://github.com/stryker-mutator/stryker-net/issues/2144")]
    public void AddRateLimiter_Extensions_Ok(Action<ResilienceStrategyBuilder<int>> configure)
    {
        var builder = new ResilienceStrategyBuilder<int>();

        configure(builder);

        builder.Build().Should().BeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddRateLimiter_AllExtensions_Ok()
    {
        foreach (var configure in Data.Select(v => v[0]).Cast<Action<ResilienceStrategyBuilder<int>>>())
        {
            var builder = new ResilienceStrategyBuilder<int>();

            configure(builder);

            GetResilienceStrategy(builder.Build()).Should().BeOfType<RateLimiterResilienceStrategy>();
        }
    }

    [Fact]
    public void AddRateLimiter_Ok()
    {
        new ResilienceStrategyBuilder().AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                QueueLimit = 10,
                PermitLimit = 10
            })
        }).Build().Should().BeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddRateLimiter_InvalidOptions_Throws()
    {
        new ResilienceStrategyBuilder().Invoking(b => b.AddRateLimiter(new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! }))
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
        new ResilienceStrategyBuilder<int>().Invoking(b => b.AddRateLimiter(new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! }))
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
        var strategy = new ResilienceStrategyBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = Mock.Of<RateLimiter>()
            })
            .Build();

        strategy.Should().BeOfType<RateLimiterResilienceStrategy>();
    }

    private static void AssertRateLimiter(ResilienceStrategyBuilder<int> builder, bool hasEvents, Action<RateLimiter>? assertLimiter = null)
    {
        var strategy = GetResilienceStrategy(builder.Build());
        strategy.Limiter.Should().NotBeNull();

        if (hasEvents)
        {
            strategy.OnLeaseRejected.Should().NotBeNull();
            strategy
                .OnLeaseRejected!(new OnRateLimiterRejectedArguments(ResilienceContext.Get(), Mock.Of<RateLimitLease>(), null))
                .Preserve().GetAwaiter().GetResult();
        }
        else
        {
            strategy.OnLeaseRejected.Should().BeNull();
        }

        assertLimiter?.Invoke(strategy.Limiter);
    }

    private static void AssertConcurrencyLimiter(ResilienceStrategyBuilder<int> builder, bool hasEvents)
    {
        var strategy = GetResilienceStrategy(builder.Build());
        strategy.Limiter.Should().BeOfType<ConcurrencyLimiter>();

        if (hasEvents)
        {
            strategy.OnLeaseRejected.Should().NotBeNull();
            strategy
                .OnLeaseRejected!(new OnRateLimiterRejectedArguments(ResilienceContext.Get(), Mock.Of<RateLimitLease>(), null))
                .Preserve().GetAwaiter().GetResult();
        }
        else
        {
            strategy.OnLeaseRejected.Should().BeNull();
        }
    }

    private static RateLimiterResilienceStrategy GetResilienceStrategy<T>(ResilienceStrategy<T> strategy)
    {
        return (RateLimiterResilienceStrategy)strategy.GetType().GetProperty("Strategy", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(strategy)!;
    }
}
