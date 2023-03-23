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
    public void Ctor_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ResiliencePropertyKey<int>(null!));
    }

    [Fact]
    public void Equality_Ok()
    {
        var key1 = new ResiliencePropertyKey<string>("dummy");
        var key2 = new ResiliencePropertyKey<string>("dummy");

        key1.Equals(key2).Should().BeTrue();
        key1.Equals(new ResiliencePropertyKey<string>("dummy2")).Should().BeFalse();
        key1.Equals(new ResiliencePropertyKey<object>("dummy")).Should().BeFalse();

        key1.Equals((object)key2).Should().BeTrue();
        key1.Equals((object)new ResiliencePropertyKey<string>("dummy2")).Should().BeFalse();

        (key1 == key2).Should().BeTrue();
        (key1 != key2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Ok()
    {
        var key1 = new ResiliencePropertyKey<string>("dummy");
        var key2 = new ResiliencePropertyKey<string>("dummy");

        key1.GetHashCode().Should().Be(key2.GetHashCode());
        key1.GetHashCode().Should().NotBe(new ResiliencePropertyKey<string>("dummy2").GetHashCode());
        key1.GetHashCode().Should().NotBe(new ResiliencePropertyKey<object>("dummy").GetHashCode());
    }
}
