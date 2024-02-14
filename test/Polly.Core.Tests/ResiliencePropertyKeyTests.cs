namespace Polly.Core.Tests;

public class ResiliencePropertyKeyTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var instance = new ResiliencePropertyKey<int>("dummy");

        instance.Key.Should().Be("dummy");
        instance.ToString().Should().Be("dummy");
    }

    [Fact]
    public void Ctor_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new ResiliencePropertyKey<int>(null!));
}
