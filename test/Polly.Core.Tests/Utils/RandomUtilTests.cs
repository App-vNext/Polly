using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class RandomUtilTests
{
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [Theory]
    public void Ctor_Ok(int? seed)
    {
        var util = new RandomUtil(seed);

        util.Invoking(u => u.NextDouble()).Should().NotThrow();
    }

    [Fact]
    public void Instance_Ok() =>
        RandomUtil.Instance.Should().NotBeNull();
}
