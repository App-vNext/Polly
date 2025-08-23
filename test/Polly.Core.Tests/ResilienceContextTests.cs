namespace Polly.Core.Tests;

public class ResilienceContextTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_Typed_Ok(bool synchronous)
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        context.Initialize<bool>(synchronous);

        context.ResultType.ShouldBe(typeof(bool));
        context.IsVoid.ShouldBeFalse();
        context.IsInitialized.ShouldBeTrue();
        context.IsSynchronous.ShouldBe(synchronous);
        context.ContinueOnCapturedContext.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_From_Ok(bool synchronous)
    {
        var cancellationToken = TestCancellation.Token;
        var context = ResilienceContextPool.Shared.Get("some-key", cancellationToken);
        context.Initialize<bool>(synchronous);
        context.ContinueOnCapturedContext = true;
        context.Properties.Set(new ResiliencePropertyKey<string>("A"), "B");
        using var cancellation = new CancellationTokenSource();
        var other = ResilienceContextPool.Shared.Get(cancellationToken);
        other.InitializeFrom(context, cancellation.Token);

        other.ResultType.ShouldBe(typeof(bool));
        other.IsVoid.ShouldBeFalse();
        other.IsInitialized.ShouldBeTrue();
        other.IsSynchronous.ShouldBe(synchronous);
        other.ContinueOnCapturedContext.ShouldBeTrue();
        other.OperationKey.ShouldBe("some-key");
        other.CancellationToken.ShouldBe(cancellation.Token);
        other.Properties.GetValue(new ResiliencePropertyKey<string>("A"), string.Empty).ShouldBe("B");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Initialize_Void_Ok(bool synchronous)
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        context.Initialize<VoidResult>(synchronous);

        context.ResultType.ShouldBe(typeof(VoidResult));
        context.IsVoid.ShouldBeTrue();
        context.IsInitialized.ShouldBeTrue();
        context.IsSynchronous.ShouldBe(synchronous);
        context.ContinueOnCapturedContext.ShouldBeFalse();
    }
}
