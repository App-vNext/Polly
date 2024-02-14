namespace Polly.Core.Tests;

public class ResilienceContextPoolTests
{
    [Fact]
    public void Shared_NotNull() =>
        ResilienceContextPool.Shared.Should().NotBeNull();

    [Fact]
    public void Shared_SameInstance() =>
        ResilienceContextPool.Shared.Should().BeSameAs(ResilienceContextPool.Shared);

    [Fact]
    public void Get_EnsureNotNull() =>
        ResilienceContextPool.Shared.Get().Should().NotBeNull();

    [Fact]
    public void Get_EnsureDefaults()
    {
        var context = ResilienceContextPool.Shared.Get();

        AssertDefaults(context);
    }

    [Fact]
    public void Get_CancellationToken_Ok()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get(token.Token);

        context.CancellationToken.Should().Be(token.Token);
    }

    [Fact]
    public void Get_ContinueOnCapturedContextDefault_ShouldBeFalse()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get();

        context.ContinueOnCapturedContext.Should().BeFalse();
    }

    [Fact]
    public void Get_ContinueOnCapturedContext_Ok()
    {
        var context = ResilienceContextPool.Shared.Get(true);

        context.ContinueOnCapturedContext.Should().Be(true);
    }

    [Fact]
    public void Get_OperationKeyContinueOnCapturedContext_Ok()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get("dummy", true, token.Token);

        context.ContinueOnCapturedContext.Should().Be(true);
        context.OperationKey.Should().Be("dummy");
        context.CancellationToken.Should().Be(token.Token);
    }

    [InlineData(null)]
    [InlineData("")]
    [InlineData("some-key")]
    [Theory]
    public void Get_OperationKeyAndCancellationToken_Ok(string? key)
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get(key!, token.Token);
        context.OperationKey.Should().Be(key);
        context.CancellationToken.Should().Be(token.Token);
    }

    [Fact]
    public async Task Get_EnsurePooled() =>
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            var context = ResilienceContextPool.Shared.Get();

            ResilienceContextPool.Shared.Return(context);

            ResilienceContextPool.Shared.Get().Should().BeSameAs(context);
        });

    [Fact]
    public void Return_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ResilienceContextPool.Shared.Return(null!));

    [Fact]
    public async Task Return_EnsureDefaults() =>
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            using var cts = new CancellationTokenSource();
            var context = ResilienceContextPool.Shared.Get();
            context.CancellationToken = cts.Token;
            context.Initialize<bool>(true);
            context.CancellationToken.Should().Be(cts.Token);
            context.Properties.Set(new ResiliencePropertyKey<int>("abc"), 10);
            ResilienceContextPool.Shared.Return(context);

            AssertDefaults(context);
        });

    private static void AssertDefaults(ResilienceContext context)
    {
        context.IsInitialized.Should().BeFalse();
        context.ContinueOnCapturedContext.Should().BeFalse();
        context.IsVoid.Should().BeFalse();
        context.ResultType.Name.Should().Be("UnknownResult");
        context.IsSynchronous.Should().BeFalse();
        context.CancellationToken.Should().Be(CancellationToken.None);
        context.Properties.Options.Should().BeEmpty();
        context.OperationKey.Should().BeNull();
    }
}
