using System;
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
        outcome.IsVoidResult.Should().BeFalse();
        outcome.TryGetResult(out var result).Should().BeTrue();
        result.Should().Be(10);
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
    }

    [Fact]
    public void Ctor_Exception_Ok()
    {
        var outcome = new Outcome<VoidResult>(new InvalidOperationException());
        outcome.HasResult.Should().BeFalse();
        outcome.Exception.Should().NotBeNull();
        outcome.IsVoidResult.Should().BeFalse();
        outcome.TryGetResult(out var result).Should().BeFalse();
    }
}
