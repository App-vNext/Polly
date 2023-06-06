using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly.TestUtils;

namespace Polly.Specs;

public class ResilienceStrategyConversionExtensionsTests
{
    private static readonly ResiliencePropertyKey<string> Incoming = new ResiliencePropertyKey<string>("incoming-key");

    private static readonly ResiliencePropertyKey<string> Executing = new ResiliencePropertyKey<string>("executing-key");

    private static readonly ResiliencePropertyKey<string> Outgoing = new ResiliencePropertyKey<string>("outgoing-key");

    private readonly TestResilienceStrategy strategy;
    private readonly ResilienceStrategy<string> genericStrategy;
    private bool synchronous;
    private bool @void;
    private Type? resultType;

    public ResilienceStrategyConversionExtensionsTests()
    {
        strategy = new TestResilienceStrategy();
        strategy.Before = (context, ct) =>
        {
            context.IsVoid.Should().Be(@void);
            context.IsSynchronous.Should().Be(synchronous);
            context.Properties.Set(Outgoing, "outgoing-value");
            context.Properties.GetValue(Incoming, string.Empty).Should().Be("incoming-value");

            if (resultType != null)
            {
                context.ResultType.Should().Be(resultType);
            }
        };

        genericStrategy = new ResilienceStrategyBuilder<string>()
            .AddStrategy(strategy)
            .Build();
    }

    [Fact]
    public void AsSyncPolicy_Ok()
    {
        @void = true;
        synchronous = true;
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        strategy.AsSyncPolicy().Execute(c =>
        {
            context[Executing.Key] = "executing-value";
        },
        context);

        AssertContext(context);
    }

    [Fact]
    public void AsSyncPolicy_Generic_Ok()
    {
        @void = false;
        synchronous = true;
        resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = genericStrategy.AsSyncPolicy().Execute(c => { context[Executing.Key] = "executing-value"; return "dummy"; }, context);
        AssertContext(context);
        result.Should().Be("dummy");
    }

    [Fact]
    public void AsSyncPolicy_Result_Ok()
    {
        @void = false;
        synchronous = true;
        resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = strategy.AsSyncPolicy().Execute(c => { context[Executing.Key] = "executing-value"; return "dummy"; }, context);

        AssertContext(context);
        result.Should().Be("dummy");
    }

    [Fact]
    public async Task AsAsyncPolicy_Ok()
    {
        @void = true;
        synchronous = false;
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        await strategy.AsAsyncPolicy().ExecuteAsync(c =>
        {
            context[Executing.Key] = "executing-value";
            return Task.CompletedTask;
        },
        context);

        AssertContext(context);
    }

    [Fact]
    public async Task AsAsyncPolicy_Generic_Ok()
    {
        @void = false;
        synchronous = false;
        resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = await genericStrategy.AsAsyncPolicy().ExecuteAsync(c =>
        {
            context[Executing.Key] = "executing-value";
            return Task.FromResult("dummy");
        },
        context);
        AssertContext(context);
        result.Should().Be("dummy");
    }

    [Fact]
    public async Task AsAsyncPolicy_Result_Ok()
    {
        @void = false;
        synchronous = false;
        resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = await strategy.AsAsyncPolicy().ExecuteAsync(c =>
        {
            context[Executing.Key] = "executing-value";
            return Task.FromResult("dummy");
        },
        context);

        AssertContext(context);
        result.Should().Be("dummy");
    }

    private static void AssertContext(Context context)
    {
        context[Outgoing.Key].Should().Be("outgoing-value");
        context[Executing.Key].Should().Be("executing-value");
    }
}
