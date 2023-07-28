using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Moq;

namespace Polly.RateLimiting.Tests;

public class RateLimiterCompositeStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<CompositeStrategyBuilder<int>>> Data = new()
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

            GetResilienceStrategy(builder.Build()).Should().BeOfType<RateLimiterResilienceStrategy>();
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
            .Should()
            .BeOfType<RateLimiterResilienceStrategy>();
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
                RateLimiter = ResilienceRateLimiter.Create(Mock.Of<RateLimiter>())
            })
            .Build();

        strategy.Should().BeOfType<RateLimiterResilienceStrategy>();
    }

    private static void AssertRateLimiter(CompositeStrategyBuilder<int> builder, bool hasEvents, Action<RateLimiter>? assertLimiter = null)
    {
        var strategy = GetResilienceStrategy(builder.Build());
        strategy.Limiter.Should().NotBeNull();

        if (hasEvents)
        {
            strategy.OnLeaseRejected.Should().NotBeNull();
            strategy
                .OnLeaseRejected!(new OnRateLimiterRejectedArguments(ResilienceContextPool.Shared.Get(), Mock.Of<RateLimitLease>(), null))
                .Preserve().GetAwaiter().GetResult();
        }
        else
        {
            strategy.OnLeaseRejected.Should().BeNull();
        }

        assertLimiter?.Invoke(strategy.Limiter.Limiter!);
    }

    private static void AssertConcurrencyLimiter(CompositeStrategyBuilder<int> builder, bool hasEvents)
    {
        var strategy = GetResilienceStrategy(builder.Build());
        strategy.Limiter.Limiter.Should().BeOfType<ConcurrencyLimiter>();

        if (hasEvents)
        {
            strategy.OnLeaseRejected.Should().NotBeNull();
            strategy
                .OnLeaseRejected!(new OnRateLimiterRejectedArguments(ResilienceContextPool.Shared.Get(), Mock.Of<RateLimitLease>(), null))
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
