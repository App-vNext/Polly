using System.ComponentModel.DataAnnotations;

namespace Polly.Core.Tests;

public class PredicateBuilderTests
{
    public static TheoryData<Action<PredicateBuilder<string>>, Outcome<string>, bool> HandleResultData = new()
    {
        { builder => builder.HandleResult("val"), Outcome.FromResult("val"), true },
        { builder => builder.HandleResult("val"), Outcome.FromResult("val2"), false },
        { builder => builder.HandleResult("val"), Outcome.FromException<string>(new InvalidOperationException()), false },
        { builder => builder.HandleResult("val", StringComparer.OrdinalIgnoreCase) ,Outcome.FromResult("VAL"), true },
        { builder => builder.HandleResult(r => r == "val"), Outcome.FromResult("val"), true },
        { builder => builder.HandleResult(r => r == "val2"), Outcome.FromResult("val"), false },
        { builder => builder.Handle<InvalidOperationException>(), Outcome.FromException<string>(new InvalidOperationException()), true },
        { builder => builder.Handle<InvalidOperationException>(), Outcome.FromException<string>(new FormatException()), false },
        { builder => builder.Handle<InvalidOperationException>(e => false), Outcome.FromException<string>(new InvalidOperationException()), false },
        { builder => builder.HandleInner<InvalidOperationException>(e => false), Outcome.FromException<string>(new InvalidOperationException()), false },
        { builder => builder.HandleInner<InvalidOperationException>(), Outcome.FromResult("value"), false },
        { builder => builder.Handle<InvalidOperationException>(), Outcome.FromResult("value"), false },
        { builder => builder.Handle<InvalidOperationException>().HandleResult("value"), Outcome.FromResult("value"), true },
        { builder => builder.Handle<InvalidOperationException>().HandleResult("value"), Outcome.FromResult("value2"), false },
        { builder => builder.HandleInner<FormatException>(), Outcome.FromException<string>(new InvalidOperationException("dummy", new FormatException() )), true },
        { builder => builder.HandleInner<ArgumentNullException>(e => false), Outcome.FromException<string>(new InvalidOperationException("dummy", new FormatException() )), false },
        { builder => builder.HandleInner<FormatException>(e => e.Message == "m"), Outcome.FromException<string>(new InvalidOperationException("dummy", new FormatException("m") )), true },
        { builder => builder.HandleInner<FormatException>(e => e.Message == "x"), Outcome.FromException<string>(new InvalidOperationException("dummy", new FormatException("m") )), false },
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
