using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Moq;
using Polly.Builder;
using Xunit;

namespace Polly.RateLimiting.Tests;

public class RateLimiterResilienceStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResilienceStrategyBuilder>> Data = new()
    {
        builder =>
        {
            var called = false;

            builder.AddConcurrencyLimiter(
                new ConcurrencyLimiterOptions
                {
                    PermitLimit = 2,
                    QueueLimit = 2
                },
                () => called = true);
            AssertConcurrencyLimiter(builder, hasEvents: true);

            called.Should().BeTrue();
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
            var called = false;

            builder.AddConcurrencyLimiter(
                new ConcurrencyLimiterOptions
                {
                    PermitLimit = 2,
                    QueueLimit = 2
                },
                configure => configure.Add(() => called = true));

            AssertConcurrencyLimiter(builder, hasEvents: true);
            called.Should().BeTrue();
        },
        builder =>
        {
            builder.AddRateLimiter(Mock.Of<RateLimiter>());
            AssertRateLimiter(builder, hasEvents: false);
        },
        builder =>
        {
            var called = false;
            builder.AddRateLimiter(Mock.Of<RateLimiter>(), () => called =true);
            AssertRateLimiter(builder, hasEvents: true);
            called.Should().BeTrue();
        },
        builder =>
        {
            var called = false;
            builder.AddRateLimiter(Mock.Of<RateLimiter>(), ev => ev.Add(() => called = true));
            AssertRateLimiter(builder, hasEvents: true);
            called.Should().BeTrue();
        }
    };

    [MemberData(nameof(Data))]
    [Theory]
    public void AddRateLimiter_Extensions_Ok(Action<ResilienceStrategyBuilder> configure)
    {
        var builder = new ResilienceStrategyBuilder();

        configure(builder);

        builder.Build().Should().BeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddRateLimiter_InvalidOptions_Throws()
    {
        new ResilienceStrategyBuilder().Invoking(b => b.AddRateLimiter(new RateLimiterStrategyOptions()))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The rate limiter strategy options are invalid.

            Validation Errors:
            The RateLimiter field is required.
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

    private static void AssertRateLimiter(ResilienceStrategyBuilder builder, bool hasEvents)
    {
        var strategy = (RateLimiterResilienceStrategy)builder.Build();
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
    }

    private static void AssertConcurrencyLimiter(ResilienceStrategyBuilder builder, bool hasEvents)
    {
        var strategy = (RateLimiterResilienceStrategy)builder.Build();
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
}
