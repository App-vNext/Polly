using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;
public class OutcomeTests
{
    [Fact]
    public void Ctor_Result_Ok()
    {
        var outcome = new Outcome<int>(10);
        outcome.HasResult.Should().BeTrue();
        outcome.Exception.Should().BeNull();
        outcome.ExceptionDispatchInfo.Should().BeNull();
        outcome.IsVoidResult.Should().BeFalse();
        outcome.TryGetResult(out var result).Should().BeTrue();
        result.Should().Be(10);
        outcome.ToString().Should().Be("10");

        outcome.AsOutcome().HasResult.Should().BeTrue();
        outcome.AsOutcome().Exception.Should().BeNull();
        outcome.AsOutcome().IsVoidResult.Should().BeFalse();
        outcome.AsOutcome().TryGetResult(out var resultObj).Should().BeTrue();
        resultObj.Should().Be(10);
    }

    [Fact]
    public void Ctor_VoidResult_Ok()
    {
        var outcome = new Outcome<VoidResult>(VoidResult.Instance);
        outcome.HasResult.Should().BeTrue();
        outcome.Exception.Should().BeNull();
        outcome.IsVoidResult.Should().BeTrue();
        outcome.TryGetResult(out var result).Should().BeFalse();
        outcome.Result.Should().Be(VoidResult.Instance);
        outcome.ToString().Should().Be("void");

        outcome.AsOutcome().HasResult.Should().BeTrue();
        outcome.AsOutcome().Exception.Should().BeNull();
        outcome.AsOutcome().IsVoidResult.Should().BeTrue();
        outcome.AsOutcome().TryGetResult(out _).Should().BeFalse();
        outcome.AsOutcome().Result.Should().Be(VoidResult.Instance);
    }

    [Fact]
    public void Ctor_Exception_Ok()
    {
        var outcome = new Outcome<VoidResult>(new InvalidOperationException("Dummy message."));
        outcome.HasResult.Should().BeFalse();
        outcome.Exception.Should().NotBeNull();
        outcome.ExceptionDispatchInfo.Should().NotBeNull();
        outcome.IsVoidResult.Should().BeFalse();
        outcome.TryGetResult(out var result).Should().BeFalse();
        outcome.ToString().Should().Be("Dummy message.");

        outcome.AsOutcome().HasResult.Should().BeFalse();
        outcome.AsOutcome().Exception.Should().NotBeNull();
        outcome.AsOutcome().IsVoidResult.Should().BeFalse();
        outcome.AsOutcome().TryGetResult(out _).Should().BeFalse();
        outcome.AsOutcome().ExceptionDispatchInfo.Should().Be(outcome.ExceptionDispatchInfo);
    }

    [Fact]
    public void ToString_NullResult_ShouldBeEmpty()
    {
        var outcome = new Outcome<object>((object)null!);
        outcome.ToString().Should().BeEmpty();
    }
}
