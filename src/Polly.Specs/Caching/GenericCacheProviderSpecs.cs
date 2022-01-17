using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Caching;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Caching
{
    [Collection(Constants.SystemClockDependentTestCollection)]
    public class GenericCacheProviderSpecs : IDisposable
    {
        [Fact]
        public void Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
        {
            const string operationKey = "SomeOperationKey";

            bool onErrorCalled = false;
            Action<Context, string, Exception> onError = (_, _, _) => { onErrorCalled = true; };

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue, onError);

            (bool cacheHit, object fromCache) = stubCacheProvider.TryGet(operationKey);
            cacheHit.Should().BeFalse();
            fromCache.Should().BeNull();

            ResultPrimitive result = cache.Execute(_ => ResultPrimitive.Substitute, new Context(operationKey));

            onErrorCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_delegate_and_put_value_in_cache_for_non_nullable_types_if_cache_does_not_hold_value()
        {
            const ResultPrimitive valueToReturn = ResultPrimitive.Substitute;
            const string operationKey = "SomeOperationKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);

            (bool cacheHit1, object fromCache1) = stubCacheProvider.TryGet(operationKey);

            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            cache.Execute(_ => valueToReturn, new Context(operationKey)).Should().Be(valueToReturn);

            (bool cacheHit2, object fromCache2) = stubCacheProvider.TryGet(operationKey);

            cacheHit2.Should().BeTrue();
            fromCache2.Should().Be(valueToReturn);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
