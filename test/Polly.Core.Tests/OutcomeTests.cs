namespace Polly.Core.Tests;

public class OutcomeTests
{
    [Fact]
    public void Ctor_Result_Ok()
    {
        int expected = 10;
        var outcome = Outcome.FromResult(expected);
        outcome.HasResult.ShouldBeTrue();
        outcome.Exception.ShouldBeNull();
        outcome.ExceptionDispatchInfo.ShouldBeNull();
        outcome.IsVoidResult.ShouldBeFalse();
        outcome.TryGetResult(out var result).ShouldBeTrue();
        result.ShouldBe(expected);
        outcome.ToString().ShouldBe("10");

#if UNION_TYPES
        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(expected);
        outcome.TryGetValue(out Exception? _).ShouldBeFalse();
        outcome.TryGetValue(out int actual).ShouldBeTrue();
        actual.ShouldBe(expected);

        int value = outcome switch
        {
            int x => x,
            Exception => Fail<int>(),
            null => Fail<int>(),
        };

        value.ShouldBe(expected);
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
        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(VoidResult.Instance);
        outcome.TryGetValue(out Exception? _).ShouldBeFalse();
        outcome.TryGetValue(out VoidResult? actual).ShouldBeTrue();
        actual.ShouldBe(VoidResult.Instance);

        VoidResult value = outcome switch
        {
#pragma warning disable CA1508
            VoidResult x => x,
#pragma warning restore CA1508
            Exception => Fail<VoidResult>(),
            null => Fail<VoidResult>(),
        };

        value.ShouldBe(VoidResult.Instance);
#endif
    }

    [Fact]
    public void Ctor_Exception_Ok()
    {
        var expected = new InvalidOperationException("Dummy message.");
        var outcome = Outcome.FromException(expected);
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldNotBeNull();
        outcome.ExceptionDispatchInfo.ShouldNotBeNull();
        outcome.IsVoidResult.ShouldBeFalse();
        outcome.TryGetResult(out var result).ShouldBeFalse();
        outcome.ToString().ShouldBe("Dummy message.");

#if UNION_TYPES
        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(expected);
        outcome.TryGetValue(out VoidResult? _).ShouldBeFalse();
        outcome.TryGetValue(out Exception? exceptionValue).ShouldBeTrue();
        exceptionValue.ShouldBe(expected);

        Exception value = outcome switch
        {
#pragma warning disable CA1508
            Exception ex => ex,
#pragma warning restore CA1508
            VoidResult => Fail<Exception>(),
            null => Fail<Exception>(),
        };

        value.ShouldNotBeNull();
        value.ShouldBe(expected);
        value.Message.ShouldBe("Dummy message.");
#endif
    }

    [Fact]
    public void Ctor_Default_Ok()
    {
        Outcome<int> outcome = default;

        outcome.Exception.ShouldBeNull();
        outcome.Result.ShouldBe(default);

#if UNION_TYPES
        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(default(int));
        outcome.TryGetValue(out Exception? _).ShouldBeFalse();
        outcome.TryGetValue(out int actual).ShouldBeTrue();
        actual.ShouldBe(default);
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
