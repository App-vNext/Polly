using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Caching;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Caching
{
    public class CacheAsyncSpecs : IDisposable
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_cache_provider_is_null()
        {
            ICacheProviderAsync cacheProvider = null;
            Action action = () => Policy.CacheAsync(cacheProvider, TimeSpan.MaxValue);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");
        }

        [Fact]
        public void Should_throw_when_ttl_strategy_is_null()
        {
            ICacheProviderAsync cacheProvider = new StubCacheProvider();
            ITtlStrategy ttlStrategy = null;
            Action action = () => Policy.CacheAsync(cacheProvider, ttlStrategy);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("ttlStrategy");
        }

        [Fact]
        public void Should_throw_when_cache_key_strategy_is_null()
        {
            ICacheProviderAsync cacheProvider = new StubCacheProvider();
            ICacheKeyStrategy cacheKeyStrategy = null;
            Action action = () => Policy.CacheAsync(cacheProvider, TimeSpan.MaxValue, cacheKeyStrategy);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("cacheKeyStrategy");
        }

        #endregion

        #region Caching behaviours

        [Fact]
        public async void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
        {
            const string valueToReturnFromCache = "valueToReturnFromCache";
            const string valueToReturnFromExecution = "valueToReturnFromExecution";
            const string executionKey = "SomeExecutionKey";

            ICacheProviderAsync stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
            await stubCacheProvider.PutAsync(executionKey, valueToReturnFromCache, TimeSpan.MaxValue, CancellationToken.None, false).ConfigureAwait(false);

            bool delegateExecuted = false;

            (await cache.ExecuteAsync(async () =>
            {
                delegateExecuted = true;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturnFromExecution;
            }, new Context(executionKey))
                .ConfigureAwait(false))
                .Should().Be(valueToReturnFromCache);

            delegateExecuted.Should().BeFalse();
        }

        [Fact]
        public async void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ICacheProviderAsync stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

            ((string) await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().BeNull();

            (await cache.ExecuteAsync(async () => { await TaskHelper.EmptyTask.ConfigureAwait(false); return valueToReturn; }, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);

            ((string)await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().Be(valueToReturn);
        }

        [Fact]
        public async void Should_execute_delegate_and_put_value_in_cache_but_when_it_expires_execute_delegate_again()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ICacheProviderAsync stubCacheProvider = new StubCacheProvider();
            TimeSpan ttl = TimeSpan.FromMinutes(30);
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, ttl);

            ((string) await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().BeNull();

            int delegateInvocations = 0;
            Func<Task<string>> func = async () =>
            {
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            DateTime fixedTime = SystemClock.UtcNow();
            SystemClock.UtcNow = () => fixedTime;

            // First execution should execute delegate and put result in the cache.
            (await cache.ExecuteAsync(func, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);
            ((string)await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().Be(valueToReturn);

            // Second execution (before cache expires) should get it from the cache - no further delegate execution.
            // (Manipulate time so just prior cache expiry).
            SystemClock.UtcNow = () => fixedTime.Add(ttl).AddTicks(-1);
            (await cache.ExecuteAsync(func, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            // Manipulate time to force cache expiry.
            SystemClock.UtcNow = () => fixedTime.Add(ttl).AddTicks(1);

            // Third execution (cache expired) should not get it from the cache - should cause further delegate execution.
            (await cache.ExecuteAsync(func, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(2);
        }

        [Fact]
        public async void Should_execute_delegate_but_not_put_value_in_cache_if_cache_does_not_hold_value_but_ttl_indicates_not_worth_caching()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ICacheProviderAsync stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.Zero);

            ((string)await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().Be(null);

            (await cache.ExecuteAsync(async () => { await TaskHelper.EmptyTask.ConfigureAwait(false); return valueToReturn; }, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);

            ((string)await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().Be(null);
        }

        [Fact]
        public async void Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

            int delegateInvocations = 0;
            Func<Task<string>> func = async () =>
            {
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            (await cache.ExecuteAsync(func, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (await cache.ExecuteAsync(func, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (await cache.ExecuteAsync(func, new Context(executionKey)).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);
        }

        [Fact]
        public async void Should_allow_custom_ICacheKeyStrategy()
        {
            ICacheProviderAsync stubCacheProvider = new StubCacheProvider();
            ICacheKeyStrategy cacheKeyStrategy = new MockCacheKeyStrategy(context => context.ExecutionKey + context["id"]);
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, cacheKeyStrategy);

            object person1 = new object();
            await stubCacheProvider.PutAsync("person1", person1, TimeSpan.MaxValue, CancellationToken.None, false).ConfigureAwait(false);
            object person2 = new object();
            await stubCacheProvider.PutAsync("person2", person2, TimeSpan.MaxValue, CancellationToken.None, false).ConfigureAwait(false);

            bool funcExecuted = false;
            Func<Task<object>> func = async () => { funcExecuted = true; await TaskHelper.EmptyTask.ConfigureAwait(false); return new object(); };

            (await cache.ExecuteAsync(func, new Context("person", new { id = "1" }.AsDictionary())).ConfigureAwait(false)).Should().BeSameAs(person1);
            funcExecuted.Should().BeFalse();

            (await cache.ExecuteAsync(func, new Context("person", new { id = "2" }.AsDictionary())).ConfigureAwait(false)).Should().BeSameAs(person2);
            funcExecuted.Should().BeFalse();
        }

        #endregion

        #region No-op pass-through behaviour

        [Fact]
        public async void Should_always_execute_delegate_if_execution_key_not_set()
        {
            string valueToReturn = Guid.NewGuid().ToString();
            
            CachePolicy cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

            int delegateInvocations = 0;
            Func<Task<string>> func = async () => {
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            (await cache.ExecuteAsync(func /*, no execution key */).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            (await cache.ExecuteAsync(func /*, no execution key */).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(2);
        }

        [Fact]
        public void Should_always_execute_delegate_if_execution_is_void_returning()
        {
            string executionKey = Guid.NewGuid().ToString();

            CachePolicy cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

            int delegateInvocations = 0;
            Func<Task> action = async () => { delegateInvocations++; await TaskHelper.EmptyTask.ConfigureAwait(false); };

            cache.ExecuteAsync(action, new Context(executionKey));
            delegateInvocations.Should().Be(1);

            cache.ExecuteAsync(action, new Context(executionKey));
            delegateInvocations.Should().Be(2);
        }

        #endregion

        #region Cancellation

        [Fact]
        public async void Should_honour_cancellation_even_if_prior_execution_has_cached()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            CachePolicy cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            int delegateInvocations = 0;
            Func<CancellationToken, Task<string>> func = async ct =>
            {
                // delegate does not observe cancellation token; test is whether CacheEngine does.
                delegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            (await cache.ExecuteAsync(func, new Context(executionKey), tokenSource.Token).ConfigureAwait(false)).Should().Be(valueToReturn);
            delegateInvocations.Should().Be(1);

            tokenSource.Cancel();

            cache.Awaiting(policy => policy.ExecuteAsync(func, new Context(executionKey), tokenSource.Token))
                .ShouldThrow<OperationCanceledException>();
            delegateInvocations.Should().Be(1);
        }

        [Fact]
        public async void Should_honour_cancellation_during_delegate_execution_and_not_put_to_cache()
        {
            const string valueToReturn = "valueToReturn";
            const string executionKey = "SomeExecutionKey";

            ICacheProviderAsync stubCacheProvider = new StubCacheProvider();
            CachePolicy cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Func<CancellationToken, Task<string>> func = async ct =>
            {
                tokenSource.Cancel(); // simulate cancellation raised during delegate execution
                ct.ThrowIfCancellationRequested();
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return valueToReturn;
            };

            cache.Awaiting(policy => policy.ExecuteAsync(func, new Context(executionKey), tokenSource.Token))
                .ShouldThrow<OperationCanceledException>();

           ((string) await stubCacheProvider.GetAsync(executionKey, CancellationToken.None, false).ConfigureAwait(false)).Should().BeNull();
        }

        #endregion

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
