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

    [Fact]
    public void FromException_Throws_If_Null() =>
        Assert.Throws<ArgumentNullException>("exception", () => Outcome.FromException<Exception>(null!));

    [Fact]
    public async Task FromExceptionAsValueTask_Throws_If_Null() =>
        await Assert.ThrowsAsync<ArgumentNullException>("exception", async () => await Outcome.FromExceptionAsValueTask<Exception>(null!));

    [Fact]
    public async Task FromValueTask_CompletedSuccessfully_ReturnsVoidSynchronously()
    {
        var result = Outcome.FromValueTask(default, continueOnCapturedContext: false);

        result.IsCompletedSuccessfully.ShouldBeTrue();

        var outcome = await result;
        outcome.IsVoidResult.ShouldBeTrue();
        outcome.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task FromValueTask_Pending_ReturnsVoidAfterCompletion()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = Outcome.FromValueTask(new ValueTask(tcs.Task), continueOnCapturedContext: false);

        result.IsCompleted.ShouldBeFalse();

        tcs.SetResult(true);

        var outcome = await result;
        outcome.IsVoidResult.ShouldBeTrue();
        outcome.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task FromValueTask_Pending_CapturesException()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var exception = new InvalidOperationException("Dummy message.");

        var result = Outcome.FromValueTask(new ValueTask(tcs.Task), continueOnCapturedContext: false);

        result.IsCompleted.ShouldBeFalse();

        tcs.SetException(exception);

        var outcome = await result;
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task FromValueTask_Faulted_CapturesException()
    {
        var exception = new InvalidOperationException("Dummy message.");

        var result = Outcome.FromValueTask(new ValueTask(Task.FromException(exception)), continueOnCapturedContext: false);

        var outcome = await result;
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task FromValueTask_Canceled_CapturesException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = Outcome.FromValueTask(new ValueTask(Task.FromCanceled(cts.Token)), continueOnCapturedContext: false);

        var outcome = await result;
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldBeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task FromValueTask_Generic_CompletedSuccessfully_ReturnsResultSynchronously()
    {
        var result = Outcome.FromValueTask(new ValueTask<int>(42), continueOnCapturedContext: false);

        result.IsCompletedSuccessfully.ShouldBeTrue();

        var outcome = await result;
        outcome.Result.ShouldBe(42);
        outcome.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task FromValueTask_Generic_Pending_ReturnsResultAfterCompletion()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = Outcome.FromValueTask(new ValueTask<int>(tcs.Task), continueOnCapturedContext: false);

        result.IsCompleted.ShouldBeFalse();

        tcs.SetResult(42);

        var outcome = await result;
        outcome.Result.ShouldBe(42);
        outcome.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task FromValueTask_Generic_Pending_CapturesException()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var exception = new InvalidOperationException("Dummy message.");

        var result = Outcome.FromValueTask(new ValueTask<int>(tcs.Task), continueOnCapturedContext: false);

        result.IsCompleted.ShouldBeFalse();

        tcs.SetException(exception);

        var outcome = await result;
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task FromValueTask_Generic_Faulted_CapturesException()
    {
        var exception = new InvalidOperationException("Dummy message.");

        var result = Outcome.FromValueTask(new ValueTask<int>(Task.FromException<int>(exception)), continueOnCapturedContext: false);

        var outcome = await result;
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task FromValueTask_Generic_Canceled_CapturesException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = Outcome.FromValueTask(new ValueTask<int>(Task.FromCanceled<int>(cts.Token)), continueOnCapturedContext: false);

        var outcome = await result;
        outcome.HasResult.ShouldBeFalse();
        outcome.Exception.ShouldBeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task FromValueTask_CompletedValueTaskSource_ConsumesSourceExactlyOnce()
    {
        var source = new CompletedValueTaskSource();

        var result = Outcome.FromValueTask(source.AsValueTask(), continueOnCapturedContext: false);

        result.IsCompletedSuccessfully.ShouldBeTrue();
        source.GetResultCalls.ShouldBe(1);

        var outcome = await result;
        outcome.IsVoidResult.ShouldBeTrue();
        outcome.Exception.ShouldBeNull();
        source.GetResultCalls.ShouldBe(1);
    }

    [Fact]
    public async Task FromValueTask_Generic_CompletedValueTaskSource_ConsumesSourceExactlyOnce()
    {
        var source = new CompletedValueTaskSource();

        var result = Outcome.FromValueTask(source.AsGenericValueTask(), continueOnCapturedContext: false);

        result.IsCompletedSuccessfully.ShouldBeTrue();
        source.GetResultCalls.ShouldBe(1);

        var outcome = await result;
        outcome.Result.ShouldBe(CompletedValueTaskSource.Value);
        outcome.Exception.ShouldBeNull();
        source.GetResultCalls.ShouldBe(1);
    }
}
