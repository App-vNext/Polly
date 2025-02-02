namespace Polly.Core.Tests;

public class ResiliencePropertyKeyTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var instance = new ResiliencePropertyKey<int>("dummy");

        instance.Key.ShouldBe("dummy");
        instance.ToString().ShouldBe("dummy");
    }

    [Fact]
    public void Ctor_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new ResiliencePropertyKey<int>(null!));
}
