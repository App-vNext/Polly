using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class MappingCacheProviderSpecs
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_wrapped_cache_provider_is_null()
        {
            ICacheProvider cacheProvider = null;
            Action action = () => new StubMappingCacheProvider_base64(cacheProvider);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("wrappedCacheProvider");
        }

        #endregion

        #region Caching behaviours

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
        {
            const string valueToReturnFromCache = "some cached http response body";
            const string valueToReturnFromExecution = "shouldn't execute delegate to get this";
            const string executionKey = "SomeExecutionKey";

            ICacheProvider mappedCacheProvider = new StubMappingCacheProvider_base64(new StubCacheProvider());
            CachePolicy cache = Policy.Cache(mappedCacheProvider);
            mappedCacheProvider.Put(executionKey, valueToReturnFromCache);

            bool delegateExecuted = false;

            String returned = cache.Execute(() =>
            {
                delegateExecuted = true;
                return valueToReturnFromExecution;
            }, new Context(executionKey));

            delegateExecuted.Should().BeFalse();
            returned.Should().Be(valueToReturnFromCache);
        }

        [Fact]
        public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
        {
            const string valueToReturn = "shouldn't execute delegate to get this";
            const string executionKey = "SomeExecutionKey";

            ICacheProvider mappedCacheProvider = new StubMappingCacheProvider_base64(new StubCacheProvider());
            CachePolicy cache = Policy.Cache(mappedCacheProvider);

            mappedCacheProvider.Get(executionKey).Should().BeNull();

            cache.Execute(() => { return valueToReturn; }, new Context(executionKey)).Should().Be(valueToReturn);

            mappedCacheProvider.Get(executionKey).Should().Be(valueToReturn);
        }

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy cache = Policy.Cache(new StubMappingCacheProvider_base64(new StubCacheProvider()));

            int delegateExecutions = 0;
            Func<string> func = () =>
            {
                delegateExecutions++;
                return valueToReturn;
            };

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateExecutions.Should().Be(1);

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateExecutions.Should().Be(1);

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateExecutions.Should().Be(1);
        }

        #endregion

        #region Composing multiple MappingCacheProviders as decorators

        [Fact]
        public void Can_compose_mapping_cache_providers_as_decorators()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy cache = Policy.Cache(new StubMappingCacheProvider_Reverse(new StubMappingCacheProvider_base64(new StubCacheProvider())));

            int delegateExecutions = 0;
            Func<string> func = () =>
            {
                delegateExecutions++;
                return valueToReturn;
            };

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateExecutions.Should().Be(1);

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateExecutions.Should().Be(1);

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateExecutions.Should().Be(1);
        }
        #endregion
    }
}
