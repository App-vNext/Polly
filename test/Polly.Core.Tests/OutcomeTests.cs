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

#if UNION_TYPES
        int value = outcome switch
        {
            int x => x,
            Exception => Fail<int>(),
            null => Fail<int>(),
        };

        value.ShouldBe(10);
#endif
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

#if UNION_TYPES
        VoidResult value = outcome switch
        {
            VoidResult x => x,
            Exception => Fail<VoidResult>(),
            null => Fail<VoidResult>(),
        };

        value.ShouldBe(VoidResult.Instance);
#endif
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

#if UNION_TYPES
        Exception value = outcome switch
        {
            Exception ex => ex,
            VoidResult => Fail<Exception>(),
            null => Fail<Exception>(),
        };

        value.ShouldNotBeNull();
        value.Message.ShouldBe("Dummy message.");
#endif
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

    [Fact]
    public void FromException_Throws_If_Null() =>
        Assert.Throws<ArgumentNullException>("exception", () => Outcome.FromException<Exception>(null!));

    [Fact]
    public async Task FromExceptionAsValueTask_Throws_If_Null() =>
        await Assert.ThrowsAsync<ArgumentNullException>("exception", async () => await Outcome.FromExceptionAsValueTask<Exception>(null!));

#if UNION_TYPES
    private static T Fail<T>()
    {
        Assert.Fail($"The outcome does not represent the correct union type.");
        return default;
    }
#endif
}
