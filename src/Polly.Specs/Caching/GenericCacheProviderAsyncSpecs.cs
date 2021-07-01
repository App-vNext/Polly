using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Caching;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Caching
{
    [Collection(Constants.SystemClockDependentTestCollection)]
    public class GenericCacheProviderAsyncSpecs : IDisposable
    {
        [Fact]
        public async Task Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
        {
            const string operationKey = "SomeOperationKey";

            bool onErrorCalled = false;
            Action<Context, string, Exception> onError = (_, _, _) => { onErrorCalled = true; };

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, onError);

            (bool cacheHit, object fromCache) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false);
            cacheHit.Should().BeFalse();
            fromCache.Should().BeNull();

            ResultPrimitive result = await cache.ExecuteAsync(async _ =>
            {
                await TaskHelper.EmptyTask;
                return ResultPrimitive.Substitute;
            }, new Context(operationKey));

            onErrorCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Should_execute_delegate_and_put_value_in_cache_for_non_nullable_types_if_cache_does_not_hold_value()
        {
            const ResultPrimitive valueToReturn = ResultPrimitive.Substitute;
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

            (bool cacheHit1, object fromCache1) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false);
            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            (await cache.ExecuteAsync(async _ =>
            {
                await TaskHelper.EmptyTask;
                return ResultPrimitive.Substitute;
            }, new Context(operationKey))).Should().Be(valueToReturn);

            (bool cacheHit2, object fromCache2) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false);
            cacheHit2.Should().BeTrue();
            fromCache2.Should().Be(valueToReturn);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
