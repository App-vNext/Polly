using Polly.Registry;

namespace Polly.Core.Tests.Registry;

public static class ConfigureBuilderContextTests
{
    [Fact]
    public static void AddReloadToken_Ok()
    {
        var context = new ConfigureBuilderContext<int>(0, "dummy", "dummy");
        using var source = new CancellationTokenSource();

        context.AddReloadToken(CancellationToken.None);
        context.AddReloadToken(source.Token);

        source.Cancel();
        context.AddReloadToken(source.Token);

        context.ReloadTokens.Count.ShouldBe(1);
    }
}
