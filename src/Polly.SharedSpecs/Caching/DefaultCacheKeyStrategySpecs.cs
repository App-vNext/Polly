using System;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class DefaultCacheKeyStrategySpecs
    {
        [Fact]
        public void Should_return_Context_OperationKey_as_cache_key()
        {
            string operationKey = "SomeKey";

            Context context = new Context(operationKey);

            DefaultCacheKeyStrategy.Instance.GetCacheKey(context)
                .Should().Be(operationKey);
        }
    }
}
