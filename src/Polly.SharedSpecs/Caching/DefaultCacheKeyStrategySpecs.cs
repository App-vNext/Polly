using System;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class DefaultCacheKeyStrategySpecs
    {
        [Fact]
        public void Should_return_Context_ExecutionKey_as_cache_key()
        {
            string executionKey = Guid.NewGuid().ToString();

            Context context = new Context(executionKey);

            DefaultCacheKeyStrategy.Instance.GetCacheKey(context)
                .Should().Be(executionKey);
        }
    }
}
