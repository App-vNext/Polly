using Polly;
using Polly.TestUtils;

namespace Polly.Specs;

public class ResiliencePipelineConversionExtensionsTests
{
    private static readonly ResiliencePropertyKey<string> Incoming = new("incoming-key");

    private static readonly ResiliencePropertyKey<string> Executing = new("executing-key");

    private static readonly ResiliencePropertyKey<string> Outgoing = new("outgoing-key");

    private readonly TestResilienceStrategy _strategy;
    private readonly ResiliencePipeline<string> _genericStrategy;
    private bool _isSynchronous;
    private bool _isVoid;

    public ResiliencePipelineConversionExtensionsTests()
    {
        _strategy = new TestResilienceStrategy
        {
            Before = (context, _) =>
            {
                context.GetType().GetProperty("IsVoid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(context).ShouldBe(_isVoid);
                context.GetType().GetProperty("IsSynchronous", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(context).ShouldBe(_isSynchronous);
                context.Properties.Set(Outgoing, "outgoing-value");
                context.Properties.GetValue(Incoming, string.Empty).ShouldBe("incoming-value");
                context.OperationKey.ShouldBe("op-key");
            }
        };

        _genericStrategy = new ResiliencePipelineBuilder<string>()
            .AddStrategy(_strategy)
            .Build();
    }

    [Fact]
    public void AsAsyncPolicy_Throws_If_Null()
    {
        // Arrange
        ResiliencePipeline strategy = null!;
        ResiliencePipeline<string> strategyGeneric = null!;

        // Act and Assert
        Should.Throw<ArgumentNullException>(strategy.AsAsyncPolicy).ParamName.ShouldBe("strategy");
        Should.Throw<ArgumentNullException>(strategyGeneric.AsAsyncPolicy).ParamName.ShouldBe("strategy");
    }

    [Fact]
    public void AsSyncPolicy_Throws_If_Null()
    {
        // Arrange
        ResiliencePipeline strategy = null!;
        ResiliencePipeline<string> strategyGeneric = null!;

        // Act and Assert
        Should.Throw<ArgumentNullException>(strategy.AsSyncPolicy).ParamName.ShouldBe("strategy");
        Should.Throw<ArgumentNullException>(strategyGeneric.AsSyncPolicy).ParamName.ShouldBe("strategy");
    }

    [Fact]
    public void AsSyncPolicy_Ok()
    {
        _isVoid = true;
        _isSynchronous = true;
        var context = new Context("op-key")
        {
            [Incoming.Key] = "incoming-value"
        };

        _strategy.AsPipeline().AsSyncPolicy().Execute(_ =>
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
        var context = new Context("op-key")
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = _genericStrategy.AsSyncPolicy().Execute(_ => { context[Executing.Key] = "executing-value"; return "dummy"; }, context);
        AssertContext(context);
        result.ShouldBe("dummy");
    }

    [Fact]
    public void AsSyncPolicy_Result_Ok()
    {
        _isVoid = false;
        _isSynchronous = true;
        var context = new Context("op-key")
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = _strategy.AsPipeline().AsSyncPolicy().Execute(_ => { context[Executing.Key] = "executing-value"; return "dummy"; }, context);

        AssertContext(context);
        result.ShouldBe("dummy");
    }

    [Fact]
    public async Task AsAsyncPolicy_Ok()
    {
        _isVoid = true;
        _isSynchronous = false;
        var context = new Context("op-key")
        {
            [Incoming.Key] = "incoming-value"
        };

        await _strategy.AsPipeline().AsAsyncPolicy().ExecuteAsync(_ =>
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
        var context = new Context("op-key")
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = await _genericStrategy.AsAsyncPolicy().ExecuteAsync(_ =>
        {
            context[Executing.Key] = "executing-value";
            return Task.FromResult("dummy");
        },
        context);
        AssertContext(context);
        result.ShouldBe("dummy");
    }

    [Fact]
    public async Task AsAsyncPolicy_Result_Ok()
    {
        _isVoid = false;
        _isSynchronous = false;
        var context = new Context("op-key")
        {
            [Incoming.Key] = "incoming-value"
        };

        var result = await _strategy.AsPipeline().AsAsyncPolicy().ExecuteAsync(_ =>
        {
            context[Executing.Key] = "executing-value";
            return Task.FromResult("dummy");
        },
        context);

        AssertContext(context);
        result.ShouldBe("dummy");
    }

    [Fact]
    public void RetryStrategy_AsSyncPolicy_Ok()
    {
        var policy = new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                ShouldHandle = _ => PredicateResult.True(),
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(1)
            })
            .Build()
            .AsSyncPolicy();

        var context = new Context("op-key")
        {
            ["retry"] = 0
        };

        policy.Execute(
            c =>
            {
                c["retry"] = (int)c["retry"] + 1;
                return "dummy";
            },
            context)
            .ShouldBe("dummy");

        context["retry"].ShouldBe(6);
    }

    private static void AssertContext(Context context)
    {
        context[Outgoing.Key].ShouldBe("outgoing-value");
        context[Executing.Key].ShouldBe("executing-value");
    }
}
