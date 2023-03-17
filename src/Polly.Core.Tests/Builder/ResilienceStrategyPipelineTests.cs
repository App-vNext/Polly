using System;
using FluentAssertions;
using Polly.Builder;
using Polly.Core.Tests.Utils;
using Xunit;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyPipelineTests
{
    [Fact]
    public void CreateAndFreezeStrategies_ArgValidation()
    {
        Assert.Throws<ArgumentNullException>(() => ResilienceStrategyPipeline.CreateAndFreezeStrategies(null!));
        Assert.Throws<ArgumentException>(() => ResilienceStrategyPipeline.CreateAndFreezeStrategies(Array.Empty<DelegatingResilienceStrategy>()));
        Assert.Throws<ArgumentException>(() => ResilienceStrategyPipeline.CreateAndFreezeStrategies(new DelegatingResilienceStrategy[] { new TestResilienceStrategy() }));
    }

    [Fact]
    public void CreateAndFreezeStrategies_EnsureStrategiesLinked()
    {
        var s1 = new TestResilienceStrategy();
        var s2 = new TestResilienceStrategy();
        var s3 = new TestResilienceStrategy();

        var pipeline = ResilienceStrategyPipeline.CreateAndFreezeStrategies(new[] { s1, s2, s3 });

        s1.Next.Should().Be(s2);
        s2.Next.Should().Be(s3);
        s3.Next.Should().Be(NullResilienceStrategy.Instance);
    }

    [Fact]
    public void Create_EnsureStrategiesFrozen()
    {
        var strategies = new[]
        {
            new TestResilienceStrategy(),
            new TestResilienceStrategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreateAndFreezeStrategies(strategies);

        foreach (var s in strategies)
        {
            Assert.Throws<InvalidOperationException>(() => s.Next = NullResilienceStrategy.Instance);
        }
    }
}
