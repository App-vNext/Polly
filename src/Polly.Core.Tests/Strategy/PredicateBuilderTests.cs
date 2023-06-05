using System.ComponentModel.DataAnnotations;
using Polly.Strategy;
using Xunit;

namespace Polly.Core.Tests.Strategy;

public class PredicateBuilderTests
{
    public static TheoryData<Action<PredicateBuilder<string>>, Outcome<string>, bool> HandleResultData = new()
    {
        { builder => builder.HandleResult("val"), new Outcome<string>("val"), true },
        { builder => builder.HandleResult("val"), new Outcome<string>("val2"), false },
        { builder => builder.HandleResult("val"), new Outcome<string>(new InvalidOperationException()), false },
        { builder => builder.HandleResult("val", StringComparer.OrdinalIgnoreCase) ,new Outcome<string>("VAL"), true },
        { builder => builder.HandleResult(r => r == "val"), new Outcome<string>("val"), true },
        { builder => builder.HandleResult(r => r == "val2"), new Outcome<string>("val"), false },
        { builder => builder.Handle<InvalidOperationException>(), new Outcome<string>(new InvalidOperationException()), true },
        { builder => builder.Handle<InvalidOperationException>(), new Outcome<string>(new FormatException()), false },
        { builder => builder.Handle<InvalidOperationException>(e => false), new Outcome<string>(new InvalidOperationException()), false },
        { builder => builder.HandleInner<InvalidOperationException>(e => false), new Outcome<string>(new InvalidOperationException()), false },
        { builder => builder.HandleInner<InvalidOperationException>(), new Outcome<string>("value"), false },
        { builder => builder.Handle<InvalidOperationException>(), new Outcome<string>("value"), false },
        { builder => builder.Handle<InvalidOperationException>().HandleResult("value"), new Outcome<string>("value"), true },
        { builder => builder.Handle<InvalidOperationException>().HandleResult("value"), new Outcome<string>("value2"), false },
        { builder => builder.HandleInner<FormatException>(), new Outcome<string>(new InvalidOperationException("dummy", new FormatException() )), true },
        { builder => builder.HandleInner<ArgumentNullException>(e => false), new Outcome<string>(new InvalidOperationException("dummy", new FormatException() )), false },
        { builder => builder.HandleInner<FormatException>(e => e.Message == "m"), new Outcome<string>(new InvalidOperationException("dummy", new FormatException("m") )), true },
        { builder => builder.HandleInner<FormatException>(e => e.Message == "x"), new Outcome<string>(new InvalidOperationException("dummy", new FormatException("m") )), false },
    };

    [MemberData(nameof(HandleResultData))]
    [Theory]
    public async Task HandleResult_Ok(Action<PredicateBuilder<string>> configure, Outcome<string> value, bool handled)
    {
        var predicate = new PredicateBuilder<string>();

        configure(predicate);

        var result = await predicate.CreatePredicate<string>()(new OutcomeArguments<string, string>(ResilienceContext.Get(), value, string.Empty));
        result.Should().Be(handled);
    }

    [Fact]
    public void CreatePredicate_NotConfigured_Throws()
    {
        var predicate = new PredicateBuilder<string>()
            .Invoking(b => b.CreatePredicate<string>())
            .Should()
            .Throw<ValidationException>()
            .WithMessage("No predicates were configured. There must be at least one predicate added.");
    }
}
