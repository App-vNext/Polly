using System;
using System.Threading;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class CacheSpecs
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_cache_provider_is_null()
        {
            ICacheProvider cacheProvider = null;
            Action action = () => Policy.Cache(cacheProvider);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");
        }

        [Fact]
        public void Should_throw_when_cache_key_strategy_is_null()
        {
            ICacheProvider cacheProvider = new StubCacheProvider();
            ICacheKeyStrategy cacheKeyStrategy = null;
            Action action = () => Policy.Cache(cacheProvider, cacheKeyStrategy);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("cacheKeyStrategy");
        }

        #endregion

        #region Caching behaviours

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string executionKey = "SomeExecutionKey";

            ICacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider);
            stubCacheProvider.Put(executionKey, valueToReturnFromCache);

            bool delegateExecuted = false;

            cache.Execute(() =>
            {
                delegateExecuted = true;
                return valueToReturnFromExecution;
            }, new Context(executionKey))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ICacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider);

            stubCacheProvider.Get(executionKey).Should().BeNull();

            cache.Execute(() => { return valueToReturn; }, new Context(executionKey)).Should().Be(valueToReturn);

            stubCacheProvider.Get(executionKey).Should().Be(valueToReturn);
        }

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy cache = Policy.Cache(new StubCacheProvider());

            int delegateInvocations = 0;
            Func<string> func = () =>
            {
                delegateInvocations++;
                return valueToReturn;
            };

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);
        }
        
        [Fact]
        public void Should_allow_custom_ICacheKeyStrategy()
        {
            ICacheProvider stubCacheProvider = new StubCacheProvider();
            ICacheKeyStrategy cacheKeyStrategy = new MockCacheKeyStrategy(context => context.ExecutionKey + context["id"]);
            CachePolicy cache = Policy.Cache(stubCacheProvider, cacheKeyStrategy);

            object person1 = new object();
            stubCacheProvider.Put("person1", person1);
            object person2 = new object();
            stubCacheProvider.Put("person2", person2);

            bool funcExecuted = false;
            Func<object> func = () => { funcExecuted = true; return new object(); };

            cache.Execute(func, new Context("person", new { id = "1" }.AsDictionary())).Should().BeSameAs(person1);
            funcExecuted.Should().BeFalse();

            cache.Execute(func, new Context("person", new { id = "2" }.AsDictionary())).Should().BeSameAs(person2);
            funcExecuted.Should().BeFalse();
        }

        #endregion

        #region No-op pass-through behaviour

        [Fact]
        public void Should_always_execute_delegate_if_execution_key_not_set()
        {
            string valueToReturn = Guid.NewGuid().ToString();

            CachePolicy cache = Policy.Cache(new StubCacheProvider());

            int delegateInvocations = 0;
            Func<string> func = () =>
            {
                delegateInvocations++;
                return valueToReturn;
            };

            cache.Execute(func /*, no execution key */).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            cache.Execute(func /*, no execution key */).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(2);
        }

        [Fact]
        public void Should_always_execute_delegate_if_execution_is_void_returning()
        {
            string executionKey = Guid.NewGuid().ToString();

            CachePolicy cache = Policy.Cache(new StubCacheProvider());

            int delegateInvocations = 0;
            Action action = () => { delegateInvocations++; };

            cache.Execute(action, new Context(executionKey));
            delegateInvocations.Should().Be(1);

            cache.Execute(action, new Context(executionKey));
            delegateInvocations.Should().Be(2);
        }

        #endregion

        #region Cancellation

        [Fact]
        public void Should_honour_cancellation_even_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy cache = Policy.Cache(new StubCacheProvider());

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            int delegateInvocations = 0;
            Func<CancellationToken, string> func = ct =>
            {
                // delegate does not observe cancellation token; test is whether CacheEngine does.
                delegateInvocations++;
                return valueToReturn;
            };

            cache.Execute(func, new Context(executionKey), tokenSource.Token).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            tokenSource.Cancel();

            cache.Invoking(policy => policy.Execute(func, new Context(executionKey), tokenSource.Token))
                .ShouldThrow<OperationCanceledException>();
            delegateInvocations.Should().Be(1);
        }

        [Fact]
        public void Should_honour_cancellation_during_delegate_execution_and_not_put_to_cache()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ICacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.Cache(stubCacheProvider);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Func<CancellationToken, string> func = ct =>
            {
                tokenSource.Cancel(); // simulate cancellation raised during delegate execution
                ct.ThrowIfCancellationRequested();
                return valueToReturn;
            };

            cache.Invoking(policy => policy.Execute(func, new Context(executionKey), tokenSource.Token))
                .ShouldThrow<OperationCanceledException>();

            stubCacheProvider.Get(executionKey).Should().BeNull();
        }

        #endregion
    }
}
