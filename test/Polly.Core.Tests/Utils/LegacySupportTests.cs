namespace Polly.Core.Tests.Utils;

public class LegacySupportTests
{
    [Fact]
    public void SetProperties_Ok()
    {
        var resilienceProperties = new ResilienceProperties();
        var oldProps = resilienceProperties.Options;
        var newProps = new Dictionary<string, object?>();

        resilienceProperties.SetProperties(newProps, out var oldProperties2);

        resilienceProperties.Options.ShouldBeSameAs(newProps);
        oldProperties2.ShouldBeSameAs(oldProps);
    }

    [Fact]
    public void SetProperties_Throws_Arguments_Null()
    {
        var resilienceProperties = new ResilienceProperties();

        Assert.Throws<ArgumentNullException>("properties", () => resilienceProperties.SetProperties(null!, out _));

        resilienceProperties = null!;

        Assert.Throws<ArgumentNullException>("resilienceProperties", () => resilienceProperties.SetProperties(new Dictionary<string, object?>(), out _));
    }
}
