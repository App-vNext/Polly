namespace Polly.Core.Tests;
public class OutcomeTests
{
    [Fact]
    public void Ctor_Result_Ok()
    {
        var outcome = Outcome.FromResult(10);
        outcome.HasResult.ShouldBeTrue();
        outcome.Exception.ShouldBeNull();
        outcome.ExceptionDispatchInfo.ShouldBeNull();
        outcome.IsVoidResult.ShouldBeFalse();
        outcome.TryGetResult(out var result).ShouldBeTrue();
        result.ShouldBe(10);
        outcome.ToString().ShouldBe("10");
    }

    [Fact]
    public void Ctor_VoidResult_Ok()
    {
        var outcome = Outcome.Void;
        outcome.HasResult.ShouldBeTrue();
        outcome.Exception.ShouldBeNull();
        outcome.IsVoidResult.ShouldBeTrue();
        outcome.TryGetResult(out var result).ShouldBeFalse();
        outcome.Result.ShouldBe(VoidResult.Instance);
        outcome.ToString().ShouldBe("void");
    }

    [Fact]
    public void Ctor_Exception_Ok()
    {
        var outcome = Outcome.FromException(new InvalidOperationException("Dummy message."));
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldNotBeNull();
        outcome.ExceptionDispatchInfo.ShouldNotBeNull();
        outcome.IsVoidResult.ShouldBeFalse();
        outcome.TryGetResult(out var result).ShouldBeFalse();
        outcome.ToString().ShouldBe("Dummy message.");
    }

    [Fact]
    public void ToString_NullResult_ShouldBeEmpty()
    {
        var outcome = Outcome.FromResult<object>(default);
        outcome.ToString().ShouldBeEmpty();
    }

    [Fact]
    public void EnsureSuccess_Result()
    {
        var outcome = Outcome.FromResult("dummy");

        Should.NotThrow(outcome.ThrowIfException);
    }

    [Fact]
    public void EnsureSuccess_Exception()
    {
        var outcome = Outcome.FromException<string>(new InvalidOperationException());

        Should.Throw<InvalidOperationException>(outcome.ThrowIfException);
    }
}
