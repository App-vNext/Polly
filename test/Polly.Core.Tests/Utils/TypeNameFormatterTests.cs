namespace Polly.Core.Tests.Utils;

public class TypeNameFormatterTests
{
    [Fact]
    public void AsString_Ok()
    {
        Polly.Utils.TypeNameFormatter.Format(typeof(string)).Should().Be("String");
        Polly.Utils.TypeNameFormatter.Format(typeof(List<string>)).Should().Be("List<String>");
        Polly.Utils.TypeNameFormatter.Format(typeof(KeyValuePair<string, string>)).Should().Be("KeyValuePair`2");
    }
}
