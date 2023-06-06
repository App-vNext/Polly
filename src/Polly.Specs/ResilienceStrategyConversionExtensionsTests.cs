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

    private readonly TestResilienceStrategy _strategy;
    private readonly ResilienceStrategy<string> _genericStrategy;
    private bool _isSynchronous;
    private bool _isVoid;
    private Type? _resultType;

    public ResilienceStrategyConversionExtensionsTests()
    {
        _strategy = new TestResilienceStrategy();
        _strategy.Before = (context, ct) =>
        {
            context.IsVoid.Should().Be(_isVoid);
            context.IsSynchronous.Should().Be(_isSynchronous);
            context.Properties.Set(Outgoing, "outgoing-value");
            context.Properties.GetValue(Incoming, string.Empty).Should().Be("incoming-value");

            if (_resultType != null)
            {
                context.ResultType.Should().Be(_resultType);
            }
        };

        _genericStrategy = new ResilienceStrategyBuilder<string>()
            .AddStrategy(_strategy)
            .Build();
    }

    [Fact]
    public void AsSyncPolicy_Ok()
    {
        _isVoid = true;
        _isSynchronous = true;
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        _strategy.AsSyncPolicy().Execute(c =>
        {
            context[Executing.Key] = "executing-value";
        },
        context);

        AssertContext(context);
    }

    [Fact]
    public void AsSyncPolicy_Generic_Ok()
    {
        _isVoid = false;
        _isSynchronous = true;
        _resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = _genericStrategy.AsSyncPolicy().Execute(c => { context[Executing.Key] = "executing-value"; return "dummy"; }, context);
        AssertContext(context);
        result.Should().Be("dummy");
    }

    [Fact]
    public void AsSyncPolicy_Result_Ok()
    {
        _isVoid = false;
        _isSynchronous = true;
        _resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = _strategy.AsSyncPolicy().Execute(c => { context[Executing.Key] = "executing-value"; return "dummy"; }, context);

        AssertContext(context);
        result.Should().Be("dummy");
    }

    [Fact]
    public async Task AsAsyncPolicy_Ok()
    {
        _isVoid = true;
        _isSynchronous = false;
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        await _strategy.AsAsyncPolicy().ExecuteAsync(c =>
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
        _isVoid = false;
        _isSynchronous = false;
        _resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = await _genericStrategy.AsAsyncPolicy().ExecuteAsync(c =>
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
        _isVoid = false;
        _isSynchronous = false;
        _resultType = typeof(string);
        var context = new Context
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = await _strategy.AsAsyncPolicy().ExecuteAsync(c =>
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
