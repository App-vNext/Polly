﻿using System;
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
    public class CacheTResultAsyncSpecs : IDisposable
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_cache_provider_is_null()
        {
            IAsyncCacheProvider cacheProvider = null;
            Action action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, TimeSpan.MaxValue);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");
        }

        [Fact]
        public void Should_throw_when_ttl_strategy_is_null()
        {
            IAsyncCacheProvider cacheProvider = new StubCacheProvider();
            ITtlStrategy ttlStrategy = null;
            Action action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, ttlStrategy);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ttlStrategy");
        }

        [Fact]
        public void Should_throw_when_cache_key_strategy_is_null()
        {
            IAsyncCacheProvider cacheProvider = new StubCacheProvider();
            Func<Context, string> cacheKeyStrategy = null;
            Action action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, TimeSpan.MaxValue, cacheKeyStrategy);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cacheKeyStrategy");
        }
        #endregion

        #region Caching behaviours

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
            await stubCacheProvider.PutAsync(operationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await cache.ExecuteAsync(async ctx =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(operationKey))
                .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
        {
            const string valueToReturn = "valueToReturn";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);

            (bool cacheHit1, object fromCache1) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            (await cache.ExecuteAsync(async ctx => { await TaskHelper.EmptyTask.ConfigureAwait(false); return valueToReturn; }, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);

            (bool cacheHit2, object fromCache2) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit2.Should().BeTrue();
            fromCache2.Should().Be(valueToReturn);
        }

        [Fact]
        public async Task Should_execute_delegate_and_put_value_in_cache_but_when_it_expires_execute_delegate_again()
        {
            const string valueToReturn = "valueToReturn";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            TimeSpan ttl = TimeSpan.FromMinutes(30);
            var cache = Policy.CacheAsync<string>(stubCacheProvider, ttl);

            (bool cacheHit1, object fromCache1) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            int delegateInvocations = 0;
            Func<Context, Task<string>> func = async ctx =>
            {
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            DateTimeOffset fixedTime = SystemClock.DateTimeOffsetUtcNow();
            SystemClock.DateTimeOffsetUtcNow = () => fixedTime;

            // First execution should execute delegate and put result in the cache.
            (await cache.ExecuteAsync(func, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (bool cacheHit2, object fromCache2) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit2.Should().BeTrue();
            fromCache2.Should().Be(valueToReturn);

            // Second execution (before cache expires) should get it from the cache - no further delegate execution.
            // (Manipulate time so just prior cache expiry).
            SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(-1);
            (await cache.ExecuteAsync(func, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            // Manipulate time to force cache expiry.
            SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(1);

            // Third execution (cache expired) should not get it from the cache - should cause further delegate execution.
            (await cache.ExecuteAsync(func, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(2);
        }

        [Fact]
        public async Task Should_execute_delegate_but_not_put_value_in_cache_if_cache_does_not_hold_value_but_ttl_indicates_not_worth_caching()
        {
            const string valueToReturn = "valueToReturn";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.Zero);

            (bool cacheHit1, object fromCache1) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            (await cache.ExecuteAsync(async ctx => { await TaskHelper.EmptyTask.ConfigureAwait(false); return valueToReturn; }, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);

            (bool cacheHit2, object fromCache2) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit2.Should().BeFalse();
            fromCache2.Should().BeNull();
        }

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string operationKey = "SomeOperationKey";

            var cache = Policy.CacheAsync<string>(new StubCacheProvider(), TimeSpan.MaxValue);

            int delegateInvocations = 0;
            Func<Context, Task<string>> func = async ctx =>
            {
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            (await cache.ExecuteAsync(func, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (await cache.ExecuteAsync(func, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (await cache.ExecuteAsync(func, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);
        }

        [Fact]
        public async Task Should_allow_custom_FuncCacheKeyStrategy()
        {
            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider, TimeSpan.MaxValue, context => context.OperationKey + context["id"]);

            object person1 = new ResultClass(ResultPrimitive.Good, "person1");
            await stubCacheProvider.PutAsync("person1", person1, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);
            object person2 = new ResultClass(ResultPrimitive.Good, "person2");
            await stubCacheProvider.PutAsync("person2", person2, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool funcExecuted = false;
            Func<Context, Task<ResultClass>> func = async ctx => { funcExecuted = true; await TaskHelper.EmptyTask.ConfigureAwait(false); return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

            (await cache.ExecuteAsync(func, new Context("person", new { id = "1" }.AsDictionary())).ConfigureAwait(false)).Should().BeSameAs(person1);
            funcExecuted.Should().BeFalse();

            (await cache.ExecuteAsync(func, new Context("person", new { id = "2" }.AsDictionary())).ConfigureAwait(false)).Should().BeSameAs(person2);
            funcExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Should_allow_custom_ICacheKeyStrategy()
        {
            Action<Context, string, Exception> noErrorHandling = (_, __, ___) => { };
            Action<Context, string> emptyDelegate = (_, __) => { };

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
            //var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider, TimeSpan.MaxValue, cacheKeyStrategy);
            var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider.AsyncFor<ResultClass>(), new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

            object person1 = new ResultClass(ResultPrimitive.Good, "person1");
            await stubCacheProvider.PutAsync("person1", person1, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);
            object person2 = new ResultClass(ResultPrimitive.Good, "person2");
            await stubCacheProvider.PutAsync("person2", person2, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool funcExecuted = false;
            Func<Context, Task<ResultClass>> func = async ctx => { funcExecuted = true; await TaskHelper.EmptyTask.ConfigureAwait(false); return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

            (await cache.ExecuteAsync(func, new Context("person", new { id = "1" }.AsDictionary())).ConfigureAwait(false)).Should().BeSameAs(person1);
            funcExecuted.Should().BeFalse();

            (await cache.ExecuteAsync(func, new Context("person", new { id = "2" }.AsDictionary())).ConfigureAwait(false)).Should().BeSameAs(person2);
            funcExecuted.Should().BeFalse();
        }

        #endregion

        #region Caching behaviours, default(TResult)

        [Fact]
        public async Task Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_reference_type()
        {
            ResultClass valueToReturn = default;
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider, TimeSpan.MaxValue);

            (bool cacheHit1, object fromCache1) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            (await cache.ExecuteAsync(async ctx => { await TaskHelper.EmptyTask.ConfigureAwait(false); return valueToReturn; }, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);

            (bool cacheHit2, object fromCache2) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit2.Should().BeTrue();
            fromCache2.Should().Be(valueToReturn);
        }

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_reference_type()
        {
            ResultClass valueToReturnFromCache = default;
            ResultClass valueToReturnFromExecution = new ResultClass(ResultPrimitive.Good);
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider, TimeSpan.MaxValue);
            await stubCacheProvider.PutAsync(operationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await cache.ExecuteAsync(async ctx =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(operationKey))
                    .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_value_type()
        {
            ResultPrimitive valueToReturn = default;
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);

            (bool cacheHit1, object fromCache1) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit1.Should().BeFalse();
            fromCache1.Should().BeNull();

            (await cache.ExecuteAsync(async ctx => { await TaskHelper.EmptyTask.ConfigureAwait(false); return valueToReturn; }, new Context(operationKey)).ConfigureAwait(false)).Should().Be(valueToReturn);

            (bool cacheHit2, object fromCache2) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit2.Should().BeTrue();
            fromCache2.Should().Be(valueToReturn);
        }

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_value_type()
        {
            ResultPrimitive valueToReturnFromCache = default;
            ResultPrimitive valueToReturnFromExecution = ResultPrimitive.Good;
            valueToReturnFromExecution.Should().NotBe(valueToReturnFromCache);
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);
            await stubCacheProvider.PutAsync(operationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await cache.ExecuteAsync(async ctx =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(operationKey))
                    .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        #endregion

        #region Generic CachePolicy in PolicyWrap

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
            var noop = Policy.NoOpAsync();
            var wrap = cache.WrapAsync(noop);

            await stubCacheProvider.PutAsync(operationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await wrap.ExecuteAsync(async ctx =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(operationKey))
                .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_innermost_in_policywrap()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
            var noop = Policy.NoOpAsync();
            var wrap = noop.WrapAsync(cache);

            await stubCacheProvider.PutAsync(operationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await wrap.ExecuteAsync(async ctx =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(operationKey))
                .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_mid_policywrap()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
            var noop = Policy.NoOpAsync<string>();
            var wrap = Policy.WrapAsync(noop, cache, noop);

            await stubCacheProvider.PutAsync(operationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await wrap.ExecuteAsync(async ctx =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(operationKey))
                .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        #endregion

        #region No-op pass-through behaviour

        [Fact]
        public async Task Should_always_execute_delegate_if_execution_key_not_set()
        {
            string valueToReturn = Guid.NewGuid().ToString();

            var cache = Policy.CacheAsync<string>(new StubCacheProvider(), TimeSpan.MaxValue);

            int delegateInvocations = 0;
            Func<Task<string>> func = async () => {
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            (await cache.ExecuteAsync(func /*, no operation key */).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (await cache.ExecuteAsync(func /*, no operation key */).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(2);
        }

        #endregion

        #region Cancellation

        [Fact]
        public async Task Should_honour_cancellation_even_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string operationKey = "SomeOperationKey";

            var cache = Policy.CacheAsync<string>(new StubCacheProvider(), TimeSpan.MaxValue);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            int delegateInvocations = 0;
            Func<Context, CancellationToken, Task<string>> func = async (ctx, ct) =>
            {
                // delegate does not observe cancellation token; test is whether CacheEngine does.
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            (await cache.ExecuteAsync(func, new Context(operationKey), tokenSource.Token).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            tokenSource.Cancel();

            cache.Awaiting(policy => policy.ExecuteAsync(func, new Context(operationKey), tokenSource.Token))
                .Should().Throw<OperationCanceledException>();
            delegateInvocations.Should().Be(1);
        }

        [Fact]
        public async Task Should_honour_cancellation_during_delegate_execution_and_not_put_to_cache()
        {
            const string valueToReturn = "valueToReturn";
            const string operationKey = "SomeOperationKey";

            IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
            var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Func<Context, CancellationToken, Task<string>> func = async (ctx, ct) =>
            {
                tokenSource.Cancel(); // simulate cancellation raised during delegate execution
                ct.ThrowIfCancellationRequested();
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            cache.Awaiting(policy => policy.ExecuteAsync(func, new Context(operationKey), tokenSource.Token))
                .Should().Throw<OperationCanceledException>();

            (bool cacheHit, object fromCache) = await stubCacheProvider.TryGetAsync(operationKey, CancellationToken.None, false).ConfigureAwait(false);
            cacheHit.Should().BeFalse();
            fromCache.Should().BeNull();
        }

        #endregion

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}