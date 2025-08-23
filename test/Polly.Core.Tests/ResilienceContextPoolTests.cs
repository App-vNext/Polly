namespace Polly.Core.Tests;

public class ResilienceContextPoolTests
{
    [Fact]
    public void Shared_NotNull() =>
        ResilienceContextPool.Shared.ShouldNotBeNull();

    [Fact]
    public void Shared_SameInstance() =>
        ResilienceContextPool.Shared.ShouldBeSameAs(ResilienceContextPool.Shared);

    [Fact]
    public void Get_EnsureNotNull() =>
        ResilienceContextPool.Shared.Get(TestCancellation.Token).ShouldNotBeNull();

    [Fact]
    public void Get_EnsureDefaults()
    {
        var cancellationToken = TestCancellation.Token;
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        AssertDefaults(context, cancellationToken);
    }

    [Fact]
    public void Get_CancellationToken_Ok()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get(token.Token);

        context.CancellationToken.ShouldBe(token.Token);
    }

    [Fact]
    public void Get_ContinueOnCapturedContextDefault_ShouldBeFalse()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        context.ContinueOnCapturedContext.ShouldBeFalse();
    }

    [Fact]
    public void Get_ContinueOnCapturedContext_Ok()
    {
        var context = ResilienceContextPool.Shared.Get(true, TestCancellation.Token);

        context.ContinueOnCapturedContext.ShouldBe(true);
    }

    [Fact]
    public void Get_OperationKeyContinueOnCapturedContext_Ok()
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get("dummy", true, token.Token);

        context.ContinueOnCapturedContext.ShouldBe(true);
        context.OperationKey.ShouldBe("dummy");
        context.CancellationToken.ShouldBe(token.Token);
    }

    [InlineData(null)]
    [InlineData("")]
    [InlineData("some-key")]
    [Theory]
    public void Get_OperationKeyAndCancellationToken_Ok(string? key)
    {
        using var token = new CancellationTokenSource();

        var context = ResilienceContextPool.Shared.Get(key!, token.Token);
        context.OperationKey.ShouldBe(key);
        context.CancellationToken.ShouldBe(token.Token);
    }

    [Fact]
    public async Task Get_EnsurePooled() =>
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            var context = ResilienceContextPool.Shared.Get(default(CancellationToken));

            ResilienceContextPool.Shared.Return(context);

            ResilienceContextPool.Shared.Get(default(CancellationToken)).ShouldBeSameAs(context);
        });

    [Fact]
    public void Return_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ResilienceContextPool.Shared.Return(null!));

    [Fact]
    public async Task Return_EnsureDefaults() =>
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            using var cts = new CancellationTokenSource();
            var context = ResilienceContextPool.Shared.Get(CancellationToken.None);
            context.CancellationToken = cts.Token;
            context.Initialize<bool>(true);
            context.CancellationToken.ShouldBe(cts.Token);
            context.Properties.Set(new ResiliencePropertyKey<int>("abc"), 10);
            ResilienceContextPool.Shared.Return(context);

            AssertDefaults(context, CancellationToken.None);
        });

    private static void AssertDefaults(ResilienceContext context, CancellationToken expectedToken)
    {
        context.IsInitialized.ShouldBeFalse();
        context.ContinueOnCapturedContext.ShouldBeFalse();
        context.IsVoid.ShouldBeFalse();
        context.ResultType.Name.ShouldBe("UnknownResult");
        context.IsSynchronous.ShouldBeFalse();
        context.CancellationToken.ShouldBe(expectedToken);
        context.Properties.Options.ShouldBeEmpty();
        context.OperationKey.ShouldBeNull();
    }
}
