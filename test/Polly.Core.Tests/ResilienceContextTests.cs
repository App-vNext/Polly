namespace Polly.Core.Tests;

public class ResilienceContextTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_Typed_Ok(bool synchronous)
    {
        var context = ResilienceContextPool.Shared.Get();
        context.Initialize<bool>(synchronous);

        context.ResultType.Should().Be<bool>();
        context.IsVoid.Should().BeFalse();
        context.IsInitialized.Should().BeTrue();
        context.IsSynchronous.Should().Be(synchronous);
        context.ContinueOnCapturedContext.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_From_Ok(bool synchronous)
    {
        var cancellationToken = CancellationToken.None;
        var context = ResilienceContextPool.Shared.Get("some-key", cancellationToken);
        context.Initialize<bool>(synchronous);
        context.ContinueOnCapturedContext = true;
        context.Properties.Set(new ResiliencePropertyKey<string>("A"), "B");
        using var cancellation = new CancellationTokenSource();
        var other = ResilienceContextPool.Shared.Get(cancellationToken);
        other.InitializeFrom(context, cancellation.Token);

        other.ResultType.Should().Be<bool>();
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

        context.ResultType.Should().Be<VoidResult>();
        context.IsVoid.Should().BeTrue();
        context.IsInitialized.Should().BeTrue();
        context.IsSynchronous.Should().Be(synchronous);
        context.ContinueOnCapturedContext.Should().BeFalse();
    }
}
