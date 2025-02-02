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
}
