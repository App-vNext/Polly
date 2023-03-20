using FluentAssertions;
using Xunit;

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
    public void Return_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ResilienceContext.Return(null!));
    }

    [Fact]
    public void Return_EnsureDefaults()
    {
        using var cts = new CancellationTokenSource();
        var context = ResilienceContext.Get();
        context.CancellationToken = cts.Token;
        context.Initialize<bool>(true);
        context.CancellationToken.Should().Be(cts.Token);
        context.Properties.Set(new ResiliencePropertyKey<int>("abc"), 10);
        ResilienceContext.Return(context);

        AssertDefaults(context);
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
    }
}
