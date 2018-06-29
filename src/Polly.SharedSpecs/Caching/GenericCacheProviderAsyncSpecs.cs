using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Caching;
using Polly.Utilities;
using Polly.Wrap;
using Xunit;

namespace Polly.Specs.Caching
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class GenericCacheProviderAsyncSpecs : IDisposable
    {
        [Fact]
        public async Task Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
        {
            const string operationKey = "SomeOperationKey";

            bool onErrorCalled = false;
            Action<Context, string, Exception> onError = (ctx, key, exc) => { onErrorCalled = true; };

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, onError);

            (await stubCacheProvider.GetAsync(operationKey, CancellationToken.None, false)).Should().BeNull();
            ResultPrimitive result = await cache.ExecuteAsync(async ctx =>
            {
                await TaskHelper.EmptyTask.ConfigureAwait(false);
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
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

            (await stubCacheProvider.GetAsync(operationKey, CancellationToken.None, false)).Should().BeNull();

            (await cache.ExecuteAsync(async ctx =>
            {
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return ResultPrimitive.Substitute;
            }, new Context(operationKey))).Should().Be(valueToReturn);

            (await stubCacheProvider.GetAsync(operationKey, CancellationToken.None, false)).Should().Be(valueToReturn);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
