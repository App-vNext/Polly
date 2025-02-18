using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Retry;

namespace Polly.Core.Tests;

public class PredicateBuilderTests
{
    public static List<(Action<PredicateBuilder<string>>, Outcome<string>, bool)> HandleResultData = new()
    {
        (builder => builder.HandleResult("val"), CreateOutcome("val"), true ),
        ( builder => builder.HandleResult("val"), CreateOutcome("val2"), false ),
        ( builder => builder.HandleResult("val"), CreateOutcome(new InvalidOperationException()), false ),
        ( builder => builder.HandleResult("val", StringComparer.OrdinalIgnoreCase), CreateOutcome("VAL"), true ),
        ( builder => builder.HandleResult(r => r == "val"), CreateOutcome("val"), true ),
        ( builder => builder.HandleResult(r => r == "val2"), CreateOutcome("val"), false ),
        ( builder => builder.Handle<InvalidOperationException>(), CreateOutcome(new InvalidOperationException()), true ),
        ( builder => builder.Handle<InvalidOperationException>(), CreateOutcome(new FormatException()), false ),
        ( builder => builder.Handle<InvalidOperationException>(e => false), CreateOutcome(new InvalidOperationException()), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(e => false), CreateOutcome(new InvalidOperationException()), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome("value"), false ),
        ( builder => builder.Handle<InvalidOperationException>(), CreateOutcome("value"), false ),
        ( builder => builder.Handle<InvalidOperationException>().HandleResult("value"), CreateOutcome("value"), true ),
        ( builder => builder.Handle<InvalidOperationException>().HandleResult("value"), CreateOutcome("value2"), false ),
        ( builder => builder.HandleInner<FormatException>(), CreateOutcome(new InvalidOperationException("dummy", new FormatException() )), true ),
        ( builder => builder.HandleInner<ArgumentNullException>(e => false), CreateOutcome(new InvalidOperationException("dummy", new FormatException() )), false ),
        ( builder => builder.HandleInner<FormatException>(e => e.Message == "m"), CreateOutcome(new InvalidOperationException("dummy", new FormatException("m") )), true ),
        ( builder => builder.HandleInner<FormatException>(e => e.Message == "x"), CreateOutcome(new InvalidOperationException("dummy", new FormatException("m") )), false ),
#pragma warning disable CA2201
        //// See https://github.com/App-vNext/Polly/issues/2161
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new InvalidOperationException("1")), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new Exception("1", new InvalidOperationException("2"))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new FormatException("1", new InvalidOperationException("2"))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new Exception("1", new Exception("2", new InvalidOperationException("3")))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new AggregateException("1", new Exception("2a"), new InvalidOperationException("2b"))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new AggregateException("1", new Exception("2", new InvalidOperationException("3")))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new AggregateException("1", new FormatException("2", new NotSupportedException("3")))), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(), CreateOutcome(new AggregateException("1")), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "3"), CreateOutcome(new AggregateException("1", new FormatException("2", new NotSupportedException("3")))), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "unreachable"), CreateOutcome(new AggregateException("1", new FormatException("2", new NotSupportedException("3")))), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "1"), CreateOutcome(new InvalidOperationException("1")), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "2"), CreateOutcome(new Exception("1", new InvalidOperationException("2"))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "3"), CreateOutcome(new Exception("1", new Exception("2", new InvalidOperationException("3")))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "2b"), CreateOutcome(new AggregateException("1", new Exception("2a"), new InvalidOperationException("2b"))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "3"), CreateOutcome(new AggregateException("1", new Exception("2", new InvalidOperationException("3")))), true ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "unreachable"), CreateOutcome(new InvalidOperationException("1")), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "unreachable"), CreateOutcome(new Exception("1", new InvalidOperationException("2"))), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "unreachable"), CreateOutcome(new Exception("1", new Exception("2", new InvalidOperationException("3")))), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "unreachable"), CreateOutcome(new AggregateException("1", new Exception("2a"), new InvalidOperationException("2b"))), false ),
        ( builder => builder.HandleInner<InvalidOperationException>(ex => ex.Message is "unreachable"), CreateOutcome(new AggregateException("1", new Exception("2", new InvalidOperationException("3")))), false ),
#pragma warning restore CA2201
    };

    [Fact]
    public void Ctor_Ok()
    {
        new PredicateBuilder().ShouldNotBeNull();
        new PredicateBuilder<string>().ShouldNotBeNull();
    }

    [Theory]
#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    //[MemberData(nameof(HandleResultData))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    [InlineData(35)]
    [InlineData(36)]
    [InlineData(37)]
#pragma warning restore xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    public void HandleResult_Ok(int index)
    {
        var (configure, value, handled) = HandleResultData[index];

        var predicate = new PredicateBuilder<string>();

        configure(predicate);

        var result = predicate.Build()(value);
        result.ShouldBe(handled);
    }

    [Fact]
    public void CreatePredicate_NotConfigured_Throws()
    {
        var exception = Should.Throw<InvalidOperationException>(() => new PredicateBuilder<string>().Build());
        exception.Message.ShouldBe("No predicates were configured. There must be at least one predicate added.");
    }

    [Fact]
    public async Task Operator_RetryStrategyOptions_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var options = new RetryStrategyOptions<string>
        {
            ShouldHandle = new PredicateBuilder<string>().HandleResult("error")
        };

        var handled = await options.ShouldHandle(new RetryPredicateArguments<string>(context, CreateOutcome("error"), 0));

        handled.ShouldBeTrue();
    }

    [Fact]
    public async Task Operator_FallbackStrategyOptions_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var options = new FallbackStrategyOptions<string>
        {
            ShouldHandle = new PredicateBuilder<string>().HandleResult("error")
        };

        var handled = await options.ShouldHandle(new(context, CreateOutcome("error")));

        handled.ShouldBeTrue();
    }

    [Fact]
    public async Task Operator_HedgingStrategyOptions_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var options = new HedgingStrategyOptions<string>
        {
            ShouldHandle = new PredicateBuilder<string>().HandleResult("error")
        };

        var handled = await options.ShouldHandle(new(context, CreateOutcome("error")));

        handled.ShouldBeTrue();
    }

    [Fact]
    public async Task Operator_AdvancedCircuitBreakerStrategyOptions_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var options = new CircuitBreakerStrategyOptions<string>
        {
            ShouldHandle = new PredicateBuilder<string>().HandleResult("error")
        };

        var handled = await options.ShouldHandle(new(context, CreateOutcome("error")));

        handled.ShouldBeTrue();
    }

    private static Outcome<string> CreateOutcome(Exception exception)
        => Outcome.FromException<string>(exception);

    private static Outcome<string> CreateOutcome(string result)
        => Outcome.FromResult(result);
}
