using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class RandomUtilTests
{
    [Fact]
    public void NextDouble_Ok() => Should.NotThrow(RandomUtil.NextDouble);

    [Fact]
    public void Next_Ok() => Should.NotThrow(() => RandomUtil.Next(42));
}
