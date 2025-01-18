namespace Polly.Core.Tests.Utils;

public class TypeNameFormatterTests
{
    [Fact]
    public void AsString_Ok()
    {
        Polly.Utils.TypeNameFormatter.Format(typeof(string)).ShouldBe("String");
        Polly.Utils.TypeNameFormatter.Format(typeof(List<string>)).ShouldBe("List<String>");
        Polly.Utils.TypeNameFormatter.Format(typeof(KeyValuePair<string, string>)).ShouldBe("KeyValuePair`2");
    }
}
