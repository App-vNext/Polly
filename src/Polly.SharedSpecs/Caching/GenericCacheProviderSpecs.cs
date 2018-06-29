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
    public class GenericCacheProviderSpecs : IDisposable
    {
        [Fact]
        public void Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
        {
            const string operationKey = "SomeOperationKey";

            bool onErrorCalled = false;
            Action<Context, string, Exception> onError = (ctx, key, exc) => { onErrorCalled = true; };

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue, onError);

            stubCacheProvider.Get(operationKey).Should().BeNull();
            ResultPrimitive result = cache.Execute(ctx => ResultPrimitive.Substitute, new Context(operationKey));

            onErrorCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_delegate_and_put_value_in_cache_for_non_nullable_types_if_cache_does_not_hold_value()
        {
            const ResultPrimitive valueToReturn = ResultPrimitive.Substitute;
            const string operationKey = "SomeOperationKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);

            stubCacheProvider.Get(operationKey).Should().BeNull();

            cache.Execute(ctx => { return valueToReturn; }, new Context(operationKey)).Should().Be(valueToReturn);

            stubCacheProvider.Get(operationKey).Should().Be(valueToReturn);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
