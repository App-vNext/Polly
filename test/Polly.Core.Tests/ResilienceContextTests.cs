namespace Polly.Core.Tests;

public class ResilienceContextTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Initialize_Typed_Ok(bool synchronous)
    {
        var context = ResilienceContextPool.Shared.Get();
        context.Initialize<bool>(synchronous);

        context.ResultType.Should().Be(typeof(bool));
        context.IsVoid.Should().BeFalse();
        context.IsInitialized.Should().BeTrue();
        context.IsSynchronous.Should().Be(synchronous);
        context.ContinueOnCapturedContext.Should().BeFalse();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Initialize_From_Ok(bool synchronous)
    {
        var context = ResilienceContextPool.Shared.Get("some-key");
        context.Initialize<bool>(synchronous);
        context.ContinueOnCapturedContext = true;
        context.Properties.Set(new ResiliencePropertyKey<string>("A"), "B");
        using var cancellation = new CancellationTokenSource();
        var other = ResilienceContextPool.Shared.Get();
        other.InitializeFrom(context, cancellation.Token);

        other.ResultType.Should().Be(typeof(bool));
        other.IsVoid.Should().BeFalse();
        other.IsInitialized.Should().BeTrue();
        other.IsSynchronous.Should().Be(synchronous);
        other.ContinueOnCapturedContext.Should().BeTrue();
        other.OperationKey.Should().Be("some-key");
        other.CancellationToken.Should().Be(cancellation.Token);
        other.Properties.GetValue(new ResiliencePropertyKey<string>("A"), string.Empty).Should().Be("B");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Initialize_Void_Ok(bool synchronous)
    {
        var context = ResilienceContextPool.Shared.Get();
        context.Initialize<VoidResult>(synchronous);

        context.ResultType.Should().Be(typeof(VoidResult));
        context.IsVoid.Should().BeTrue();
        context.IsInitialized.Should().BeTrue();
        context.IsSynchronous.Should().Be(synchronous);
        context.ContinueOnCapturedContext.Should().BeFalse();
    }
}
