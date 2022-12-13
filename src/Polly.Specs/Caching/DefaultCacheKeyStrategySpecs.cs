using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching;

public class DefaultCacheKeyStrategySpecs
{
    [Fact]
    public void Should_return_Context_OperationKey_as_cache_key()
    {
        var operationKey = "SomeKey";

        var context = new Context(operationKey);

        DefaultCacheKeyStrategy.Instance.GetCacheKey(context)
            .Should().Be(operationKey);
    }
}