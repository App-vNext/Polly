namespace Polly.Core.Tests;

public class ResiliencePropertiesTests
{
    [Fact]
    public void TryGetValue_Ok()
    {
        var key = new ResiliencePropertyKey<long>("dummy");
        var props = new ResilienceProperties();

        props.Set(key, 12345);

        props.TryGetValue(key, out var val).Should().Be(true);
        val.Should().Be(12345);
    }

    [Fact]
    public void TryGetValue_NotFound_Ok()
    {
        var key = new ResiliencePropertyKey<long>("dummy");
        var props = new ResilienceProperties();

        props.TryGetValue(key, out var val).Should().Be(false);
    }

    [Fact]
    public void GetValue_Ok()
    {
        var key = new ResiliencePropertyKey<long>("dummy");
        var props = new ResilienceProperties();

        props.Set(key, 12345);

        props.GetValue(key, default).Should().Be(12345);
    }

    [Fact]
    public void GetValue_NotFound_EnsureDefault()
    {
        var key = new ResiliencePropertyKey<long>("dummy");
        var props = new ResilienceProperties();

        props.GetValue(key, -1).Should().Be(-1);
    }

    [Fact]
    public void TryGetValue_IncorrectType_NotFound()
    {
        var key1 = new ResiliencePropertyKey<long>("dummy");
        var key2 = new ResiliencePropertyKey<string>("dummy");

        var props = new ResilienceProperties();

        props.Set(key1, 12345);

        props.TryGetValue(key2, out var val).Should().Be(false);
    }

    [Fact]
    public void Clear_Ok()
    {
        var key1 = new ResiliencePropertyKey<long>("dummy");
        var key2 = new ResiliencePropertyKey<string>("dummy");

        var props = new ResilienceProperties();
        props.Set(key1, 12345);
        props.Clear();

        props.Options.Should().HaveCount(0);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Replace_Ok(bool isRawDictionary)
    {
        var key1 = new ResiliencePropertyKey<string>("A");
        var key2 = new ResiliencePropertyKey<string>("B");

        var props = new ResilienceProperties();
        props.Set(key1, "A");

        var otherProps = new ResilienceProperties();
        if (!isRawDictionary)
        {
            otherProps.Options = new ConcurrentDictionary<string, object?>();
        }

        otherProps.Set(key2, "B");

        props.Replace(otherProps);
        props.Options.Should().HaveCount(1);
        props.GetValue(key2, "").Should().Be("B");
    }
}
