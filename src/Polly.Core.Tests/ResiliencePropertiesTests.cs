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

        props.Should().HaveCount(0);
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
            otherProps.Options = new ResilienceProperties();
        }

        otherProps.Set(key2, "B");

        props.Replace(otherProps);
        props.Should().HaveCount(1);
        props.GetValue(key2, "").Should().Be("B");
    }

    [Fact]
    public void DictionaryOperations_Ok()
    {
        IDictionary<string, object?> dict = new ResilienceProperties();

        dict.TryGetValue("xyz", out var _).Should().BeFalse();
        dict.GetEnumerator().Should().NotBeNull();
        ((IEnumerable)dict).GetEnumerator().Should().NotBeNull();
        dict.IsReadOnly.Should().BeFalse();
        dict.Count.Should().Be(0);
        dict.Add("dummy", 12345L);
        dict.Values.Should().Contain(12345L);
        dict.Keys.Should().Contain("dummy");
        dict.ContainsKey("dummy").Should().BeTrue();
        dict.Contains(new KeyValuePair<string, object?>("dummy", 12345L)).Should().BeTrue();
        dict.Add("dummy2", "xyz");
        dict["dummy2"].Should().Be("xyz");
        dict["dummy3"] = "abc";
        dict["dummy3"].Should().Be("abc");
        dict.Remove("dummy2").Should().BeTrue();
        dict.Remove(new KeyValuePair<string, object?>("not-exists", "abc")).Should().BeFalse();
        dict.Clear();
        dict.Count.Should().Be(0);
        dict.Add(new KeyValuePair<string, object?>("dummy", "abc"));
        var array = new KeyValuePair<string, object?>[1];
        dict.CopyTo(array, 0);
        array[0].Key.Should().Be("dummy");
        array[0].Value.Should().Be("abc");
    }
}
