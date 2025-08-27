using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using NSubstitute;
using Polly.Registry;
using Polly.Testing;
using Polly.TestUtils;

namespace Polly.RateLimiting.Tests;

public class RateLimiterResiliencePipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly TheoryData<Action<ResiliencePipelineBuilder>> Data = new()
    {
        builder =>
        {
            builder.AddConcurrencyLimiter(2, 2);
            AssertRateLimiterStrategy(builder, strategy => strategy.Wrapper!.Limiter.ShouldBeOfType<ConcurrencyLimiter>());
        },
        builder =>
        {
            builder.AddConcurrencyLimiter(
                new ConcurrencyLimiterOptions
                {
                    PermitLimit = 2,
                    QueueLimit = 2
                });

            AssertRateLimiterStrategy(builder, strategy => strategy.Wrapper!.Limiter.ShouldBeOfType<ConcurrencyLimiter>());
        },
        builder =>
        {
            var expected = Substitute.For<RateLimiter>();
            builder.AddRateLimiter(expected);
            AssertRateLimiterStrategy(builder, strategy => strategy.Wrapper.ShouldBeNull());
        },
        builder =>
        {
            var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = 1 });
            builder.AddRateLimiter(limiter);
            builder.Build().Execute(() => { });
            AssertRateLimiterStrategy(builder, strategy => strategy.Wrapper.ShouldBeNull());
        }
    };
#pragma warning restore IDE0028

    [Theory(Skip = "https://github.com/stryker-mutator/stryker-net/issues/2144")]
#pragma warning disable xUnit1045
    [MemberData(nameof(Data))]
#pragma warning restore xUnit1045
    public void AddRateLimiter_Extensions_Ok(Action<ResiliencePipelineBuilder> configure)
    {
        var builder = new ResiliencePipelineBuilder();

        configure(builder);

        builder.Build().ShouldBeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddConcurrencyLimiter_InvalidOptions_Throws() =>
        Assert.Throws<ArgumentException>(() =>
        {
            return new ResiliencePipelineBuilder().AddConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = -10,
                QueueLimit = -10
            })
            .Build();
        });

    [Fact]
    public void AddRateLimiter_AllExtensions_Ok()
    {
        foreach (Action<ResiliencePipelineBuilder> configure in Data)
        {
            var builder = new ResiliencePipelineBuilder();

            configure(builder);

            AssertRateLimiterStrategy(builder);
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

        new ResiliencePipelineBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = args => limiter.AcquireAsync(1, args.Context.CancellationToken)
            })
            .Build()
            .GetPipelineDescriptor()
            .FirstStrategy
            .StrategyInstance
            .ShouldBeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddRateLimiter_InvalidOptions_Throws()
    {
        var options = new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! };
        var builder = new ResiliencePipelineBuilder();

        var exception = Should.Throw<ValidationException>(() => builder.AddRateLimiter(options));
        exception.Message.Trim().ShouldBe("""
            The 'RateLimiterStrategyOptions' are invalid.
            Validation Errors:
            The DefaultRateLimiterOptions field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }

    [Fact]
    public void AddGenericRateLimiter_InvalidOptions_Throws()
    {
        var options = new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! };
        var builder = new ResiliencePipelineBuilder<int>();

        var exception = Should.Throw<ValidationException>(() => builder.AddRateLimiter(options));
        exception.Message.Trim().ShouldBe("""
            The 'RateLimiterStrategyOptions' are invalid.
            Validation Errors:
            The DefaultRateLimiterOptions field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }

    [Fact]
    public void AddRateLimiter_Options_Ok()
    {
        var strategy = new ResiliencePipelineBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = args => new ValueTask<RateLimitLease>(Substitute.For<RateLimitLease>())
            })
            .Build()
            .GetPipelineDescriptor()
            .FirstStrategy
            .StrategyInstance
            .ShouldBeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public async Task DisposeRegistry_EnsureRateLimiterDisposed()
    {
        var registry = new ResiliencePipelineRegistry<string>();
        var pipeline = registry.GetOrAddPipeline("limiter", p => p.AddRateLimiter(new RateLimiterStrategyOptions()));

        var strategy = (RateLimiterResilienceStrategy)pipeline.GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        await registry.DisposeAsync();

        Should.Throw<ObjectDisposedException>(() => strategy.AsPipeline().Execute(() => { }));
    }

    [Fact]
    public void TypedBuilder_AddConcurrencyLimiter_BuildsRateLimiterStrategy()
    {
        var pipeline = new ResiliencePipelineBuilder<int>()
            .AddConcurrencyLimiter(permitLimit: 2, queueLimit: 1);

        pipeline.Build()
            .GetPipelineDescriptor()
            .FirstStrategy
            .StrategyInstance
            .ShouldBeOfType<RateLimiterResilienceStrategy>();
    }

    [Fact]
    public void AddRateLimiter_WithNullLimiter_Throws()
    {
        var builder = new ResiliencePipelineBuilder();
        Should.Throw<ArgumentNullException>(() => builder.AddRateLimiter(limiter: null!));
    }

    [Fact]
    public void AddRateLimiter_WithNullOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder();
        Should.Throw<ArgumentNullException>(() => builder.AddRateLimiter(options: null!));
    }

    [Fact]
    public void AddConcurrencyLimiter_WithNullOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder();
        Should.Throw<ArgumentNullException>(() => builder.AddConcurrencyLimiter(options: null!));
    }

    [Fact]
    public async Task RegistryDispose_DoesNotDisposeExternalLimiter()
    {
        using var externalLimiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = 1,
            QueueLimit = 0
        });

        var registry = new ResiliencePipelineRegistry<string>();
        _ = registry.GetOrAddPipeline("ext", p => p.AddRateLimiter(externalLimiter));

        await registry.DisposeAsync();

        await Should.NotThrowAsync(async () =>
        {
            var lease = await externalLimiter.AcquireAsync(1, System.Threading.CancellationToken.None);
            try
            {
                lease.IsAcquired.ShouldBeTrue();
            }
            finally
            {
                lease.Dispose();
            }
        });
    }

    private static void AssertRateLimiterStrategy(ResiliencePipelineBuilder builder, Action<RateLimiterResilienceStrategy>? assert = null, bool hasEvents = false)
    {
        ResiliencePipeline strategy = builder.Build();

        var limiterStrategy = (RateLimiterResilienceStrategy)strategy.GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        assert?.Invoke(limiterStrategy);

        if (hasEvents)
        {
            limiterStrategy.OnLeaseRejected.ShouldNotBeNull();
            limiterStrategy
                .OnLeaseRejected!(new OnRateLimiterRejectedArguments(ResilienceContextPool.Shared.Get(), Substitute.For<RateLimitLease>()))
                .Preserve().GetAwaiter().GetResult();
        }
        else
        {
            limiterStrategy.OnLeaseRejected.ShouldBeNull();
        }

        strategy
            .GetPipelineDescriptor()
            .FirstStrategy
            .StrategyInstance
            .ShouldBeOfType<RateLimiterResilienceStrategy>();
    }
}
