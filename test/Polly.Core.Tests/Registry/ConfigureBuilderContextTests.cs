using Polly.Registry;

namespace Polly.Core.Tests.Registry;

public class ConfigureBuilderContextTests
{
    [Fact]
    public void AddReloadToken_Ok()
    {
        var context = new ConfigureBuilderContext<int>(0, "dummy", "dummy");
        using var source = new CancellationTokenSource();

        context.AddReloadToken(CancellationToken.None);
        context.AddReloadToken(source.Token);

        source.Cancel();
        context.AddReloadToken(source.Token);

        context.ReloadTokens.Should().HaveCount(1);
    }
}
