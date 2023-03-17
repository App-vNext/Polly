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
        Assert.Throws<ArgumentNullException>(() => ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(null!));
        Assert.Throws<InvalidOperationException>(() => ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(Array.Empty<IResilienceStrategy>()));
        Assert.Throws<InvalidOperationException>(() => ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(new IResilienceStrategy[] { new TestResilienceStrategy() }));
        Assert.Throws<InvalidOperationException>(() => ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(new IResilienceStrategy[]
        {
            NullResilienceStrategy.Instance,
            NullResilienceStrategy.Instance
        }));
    }

    [Fact]
    public void CreateAndFreezeStrategies_EnsureStrategiesLinked()
    {
        var s1 = new TestResilienceStrategy();
        var s2 = new TestResilienceStrategy();
        var s3 = new TestResilienceStrategy();

        var pipeline = ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(new[] { s1, s2, s3 });

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

        var pipeline = ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(strategies);

        foreach (var s in strategies)
        {
            Assert.Throws<InvalidOperationException>(() => s.Next = NullResilienceStrategy.Instance);
        }
    }

    [Fact]
    public void Create_EnsureOriginalStrategiesPreserved()
    {
        var strategies = new IResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new Strategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreatePipelineAndFreezeStrategies(strategies);

        for (int i = 0; i < strategies.Length; i++)
        {
            pipeline.Strategies[i].Should().BeSameAs(strategies[i]);
        }

        pipeline.Strategies.SequenceEqual(strategies).Should().BeTrue();
    }

    private class Strategy : IResilienceStrategy
    {
        ValueTask<TResult> IResilienceStrategy.ExecuteInternalAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
        {
            throw new NotSupportedException();
        }
    }
}
