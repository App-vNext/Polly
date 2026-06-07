using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class OutcomeUtilitiesTests
{
    [Fact]
    public void WithCallerCancellation_CallerCancelled_DifferentToken_RewritesToCallerToken()
    {
        using var caller = new CancellationTokenSource();
        using var other = new CancellationTokenSource();
        caller.Cancel();
        var original = new OperationCanceledException(other.Token);
        var outcome = Outcome.FromException<int>(original);

        var result = outcome.WithCallerCancellationToken(caller.Token);

        var exception = result.Exception.ShouldBeOfType<OperationCanceledException>();
        exception.CancellationToken.ShouldBe(caller.Token);
        exception.InnerException.ShouldBeSameAs(original);
        exception.StackTrace.ShouldNotBeNull();
    }

    [Fact]
    public void WithCallerCancellation_CallerCancelled_SameToken_ReturnsOriginal()
    {
        using var caller = new CancellationTokenSource();
        caller.Cancel();
        var original = new OperationCanceledException(caller.Token);
        var outcome = Outcome.FromException<int>(original);

        var result = outcome.WithCallerCancellationToken(caller.Token);

        result.Exception.ShouldBeSameAs(original);
    }

    [Fact]
    public void WithCallerCancellation_CallerCancelled_NonCancellationException_ReturnsOriginal()
    {
        using var caller = new CancellationTokenSource();
        caller.Cancel();
        var original = new InvalidOperationException();
        var outcome = Outcome.FromException<int>(original);

        var result = outcome.WithCallerCancellationToken(caller.Token);

        result.Exception.ShouldBeSameAs(original);
    }

    [Fact]
    public void WithCallerCancellation_CallerCancelled_SuccessOutcome_ReturnsOriginal()
    {
        using var caller = new CancellationTokenSource();
        caller.Cancel();
        var outcome = Outcome.FromResult(42);

        var result = outcome.WithCallerCancellationToken(caller.Token);

        result.Exception.ShouldBeNull();
        result.Result.ShouldBe(42);
    }

    [Fact]
    public void WithCallerCancellation_CallerNotCancelled_ReturnsOriginal()
    {
        using var caller = new CancellationTokenSource();
        using var other = new CancellationTokenSource();
        other.Cancel();
        var original = new OperationCanceledException(other.Token);
        var outcome = Outcome.FromException<int>(original);

        var result = outcome.WithCallerCancellationToken(caller.Token);

        result.Exception.ShouldBeSameAs(original);
    }
}
