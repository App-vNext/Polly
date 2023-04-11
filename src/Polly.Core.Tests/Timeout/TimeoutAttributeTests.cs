using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutAttributeTests
{
    [Fact]
    public void IsValid_Object_True()
    {
        new TimeoutAttribute().IsValid(new object()).Should().BeTrue();
    }
}
