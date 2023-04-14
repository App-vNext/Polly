using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;
public class OutcomeTests
{
    [Fact]
    public void Ctor_Result_Ok()
    {
        var outcome = new Outcome(typeof(int), 10);

        outcome.Result.Should().Be(10);
        outcome.HasResult.Should().BeTrue();
        outcome.Exception.Should().BeNull();
        outcome.IsVoidResult.Should().BeFalse();
        outcome.TryGetResult(out var resultObj).Should().BeTrue();
        outcome.ResultType.Should().Be(typeof(int));
        resultObj.Should().Be(10);
        outcome.ToString().Should().Be("10");
    }

    [Fact]
    public void Ctor_Exception_Ok()
    {
        var outcome = new Outcome(typeof(int), new InvalidOperationException("Dummy message."));

        outcome.HasResult.Should().BeFalse();
        outcome.Exception.Should().NotBeNull();
        outcome.IsVoidResult.Should().BeFalse();
        outcome.TryGetResult(out _).Should().BeFalse();
        outcome.ResultType.Should().Be(typeof(int));
        outcome.ToString().Should().Be("Dummy message.");
    }

    [Fact]
    public void ToString_NullResult_ShouldBeEmpty()
    {
        var outcome = new Outcome(typeof(object), (object)null!);
        outcome.ToString().Should().BeEmpty();
    }
}
