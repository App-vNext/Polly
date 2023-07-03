using Polly.Telemetry;

namespace Polly.Core.Tests;

public class ResilienceContextTests
{
    [Fact]
    public void Get_EnsureNotNull()
    {
        ResilienceContext.Get().Should().NotBeNull();
    }

    [Fact]
    public void Get_EnsureDefaults()
    {
        var context = ResilienceContext.Get();

        AssertDefaults(context);
    }

    [Fact]
    public void Get_CancellationToken_Ok()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContext.Get(token.Token);

        context.CancellationToken.Should().Be(token.Token);
    }

    [InlineData(null)]
    [InlineData("")]
    [InlineData("some-key")]
    [Theory]
    public void Get_OperationKeyAndCancellationToken_Ok(string? key)
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContext.Get(key!, token.Token);
        context.OperationKey.Should().Be(key);
        context.CancellationToken.Should().Be(token.Token);
    }

    [Fact]
    public async Task Get_EnsurePooled()
    {
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            var context = ResilienceContext.Get();

            ResilienceContext.Return(context);

            ResilienceContext.Get().Should().BeSameAs(context);
        });
    }

    [Fact]
    public void Return_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ResilienceContext.Return(null!));
    }

    [Fact]
    public void AddResilienceEvent_Ok()
    {
        var context = ResilienceContext.Get();

        context.AddResilienceEvent(new ResilienceEvent(ResilienceEventSeverity.Information, "Dummy"));

        context.ResilienceEvents.Should().HaveCount(1);
        context.ResilienceEvents.Should().Contain(new ResilienceEvent(ResilienceEventSeverity.Information, "Dummy"));
    }

    [Fact]
    public async Task Return_EnsureDefaults()
    {
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            using var cts = new CancellationTokenSource();
            var context = ResilienceContext.Get();
            context.CancellationToken = cts.Token;
            context.Initialize<bool>(true);
            context.CancellationToken.Should().Be(cts.Token);
            context.Properties.Set(new ResiliencePropertyKey<int>("abc"), 10);
            context.AddResilienceEvent(new ResilienceEvent(ResilienceEventSeverity.Information, "dummy"));
            ResilienceContext.Return(context);

            AssertDefaults(context);
        });
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Initialize_Typed_Ok(bool synchronous)
    {
        var context = ResilienceContext.Get();
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
        var context = ResilienceContext.Get("some-key");
        context.Initialize<bool>(synchronous);
        context.ContinueOnCapturedContext = true;

        var other = ResilienceContext.Get();
        other.InitializeFrom(context);

        other.ResultType.Should().Be(typeof(bool));
        other.IsVoid.Should().BeFalse();
        other.IsInitialized.Should().BeTrue();
        other.IsSynchronous.Should().Be(synchronous);
        other.ContinueOnCapturedContext.Should().BeTrue();
        other.OperationKey.Should().Be("some-key");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Initialize_Void_Ok(bool synchronous)
    {
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(synchronous);

        context.ResultType.Should().Be(typeof(VoidResult));
        context.IsVoid.Should().BeTrue();
        context.IsInitialized.Should().BeTrue();
        context.IsSynchronous.Should().Be(synchronous);
        context.ContinueOnCapturedContext.Should().BeFalse();
    }

    private static void AssertDefaults(ResilienceContext context)
    {
        context.IsInitialized.Should().BeFalse();
        context.ContinueOnCapturedContext.Should().BeFalse();
        context.IsVoid.Should().BeFalse();
        context.ResultType.Name.Should().Be("UnknownResult");
        context.IsSynchronous.Should().BeFalse();
        context.CancellationToken.Should().Be(CancellationToken.None);
        context.Properties.Should().BeEmpty();
        context.ResilienceEvents.Should().BeEmpty();
        context.OperationKey.Should().BeNull();
    }
}
