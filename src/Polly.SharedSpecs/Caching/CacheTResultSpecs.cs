using System;
using System.Threading;
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
    public class CacheTResultSpecs : IDisposable
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_cache_provider_is_null()
        {
            ISyncCacheProvider cacheProvider = null;
            Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, TimeSpan.MaxValue);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");
        }

        [Fact]
        public void Should_throw_when_ttl_strategy_is_null()
        {
            ISyncCacheProvider cacheProvider = new StubCacheProvider();
            ITtlStrategy ttlStrategy = null;
            Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("ttlStrategy");
        }
        [Fact]
        public void Should_throw_when_cache_key_strategy_is_null()
        {
            ISyncCacheProvider cacheProvider = new StubCacheProvider();
            Func<Context, string> cacheKeyStrategy = null;
            Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, TimeSpan.MaxValue, cacheKeyStrategy);
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

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
            stubCacheProvider.Put(executionKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

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

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);

            stubCacheProvider.Get(executionKey).Should().BeNull();

            cache.Execute(() => { return valueToReturn; }, new Context(executionKey)).Should().Be(valueToReturn);

            stubCacheProvider.Get(executionKey).Should().Be(valueToReturn);
        }

        [Fact]
        public void Should_execute_delegate_and_put_value_in_cache_but_when_it_expires_execute_delegate_again()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            TimeSpan ttl = TimeSpan.FromMinutes(30);
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, ttl);

            stubCacheProvider.Get(executionKey).Should().BeNull();

            int delegateInvocations = 0;
            Func<string> func = () =>
            {
                delegateInvocations++;
                return valueToReturn;
            };

            DateTimeOffset fixedTime = SystemClock.DateTimeOffsetUtcNow();
            SystemClock.DateTimeOffsetUtcNow = () => fixedTime;

            // First execution should execute delegate and put result in the cache.
            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);
            stubCacheProvider.Get(executionKey).Should().Be(valueToReturn);

            // Second execution (before cache expires) should get it from the cache - no further delegate execution.
            // (Manipulate time so just prior cache expiry).
            SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(-1);
            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            // Manipulate time to force cache expiry.
            SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(1);

            // Third execution (cache expired) should not get it from the cache - should cause further delegate execution.
            cache.Execute(func, new Context(executionKey)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(2);
        }

        [Fact]
        public void Should_execute_delegate_but_not_put_value_in_cache_if_cache_does_not_hold_value_but_ttl_indicates_not_worth_caching()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.Zero);

            stubCacheProvider.Get(executionKey).Should().BeNull();

            cache.Execute(() => { return valueToReturn; }, new Context(executionKey)).Should().Be(valueToReturn);

            stubCacheProvider.Get(executionKey).Should().Be(null);
        }

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy<string> cache = Policy.Cache<string>(new StubCacheProvider(), TimeSpan.MaxValue);

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
        public void Should_allow_custom_FuncICacheKeyStrategy()
        {

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider, TimeSpan.MaxValue, context => context.ExecutionKey + context["id"]);

            object person1 = new ResultClass(ResultPrimitive.Good, "person1");
            stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
            object person2 = new ResultClass(ResultPrimitive.Good, "person2");
            stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

            bool funcExecuted = false;
            Func<ResultClass> func = () => { funcExecuted = true; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

            cache.Execute(func, new Context("person", new { id = "1" }.AsDictionary())).Should().BeSameAs(person1);
            funcExecuted.Should().BeFalse();

            cache.Execute(func, new Context("person", new { id = "2" }.AsDictionary())).Should().BeSameAs(person2);
            funcExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_allow_custom_ICacheKeyStrategy()
        {
            Action<Context, string, Exception> noErrorHandling = (_, __, ___) => { };
            Action<Context, string> emptyDelegate = (_, __) => { };

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.ExecutionKey + context["id"]);
            CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider.For<ResultClass>(), new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

            object person1 = new ResultClass(ResultPrimitive.Good, "person1");
            stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
            object person2 = new ResultClass(ResultPrimitive.Good, "person2");
            stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

            bool funcExecuted = false;
            Func<ResultClass> func = () => { funcExecuted = true; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

            cache.Execute(func, new Context("person", new { id = "1" }.AsDictionary())).Should().BeSameAs(person1);
            funcExecuted.Should().BeFalse();

            cache.Execute(func, new Context("person", new { id = "2" }.AsDictionary())).Should().BeSameAs(person2);
            funcExecuted.Should().BeFalse();
        }

        #endregion

        #region Generic CachePolicy in PolicyWrap

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string executionKey = "SomeExecutionKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
            Policy noop = Policy.NoOp();
            PolicyWrap<string> wrap = cache.Wrap(noop);

            stubCacheProvider.Put(executionKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

            bool delegateExecuted = false;

            wrap.Execute(() =>
            {
                delegateExecuted = true;
                return valueToReturnFromExecution;
            }, new Context(executionKey))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_innermost_in_policywrap()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string executionKey = "SomeExecutionKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
            Policy noop = Policy.NoOp();
            PolicyWrap<string> wrap = noop.Wrap(cache);

            stubCacheProvider.Put(executionKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

            bool delegateExecuted = false;

            wrap.Execute(() =>
            {
                delegateExecuted = true;
                return valueToReturnFromExecution;
            }, new Context(executionKey))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_mid_policywrap()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string executionKey = "SomeExecutionKey";

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
            Policy<string> noop = Policy.NoOp<string>();
            PolicyWrap<string> wrap = Policy.Wrap(noop, cache, noop);

            stubCacheProvider.Put(executionKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

            bool delegateExecuted = false;

            wrap.Execute(() =>
            {
                delegateExecuted = true;
                return valueToReturnFromExecution;
            }, new Context(executionKey))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        #endregion

        #region No-op pass-through behaviour

        [Fact]
        public void Should_always_execute_delegate_if_execution_key_not_set()
        {
            string valueToReturn = Guid.NewGuid().ToString();

            CachePolicy<string> cache = Policy.Cache<string>(new StubCacheProvider(), TimeSpan.MaxValue);

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

        #endregion

        #region Cancellation

        [Fact]
        public void Should_honour_cancellation_even_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy<string> cache = Policy.Cache<string>(new StubCacheProvider(), TimeSpan.MaxValue);

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

            ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
            CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);

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

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
