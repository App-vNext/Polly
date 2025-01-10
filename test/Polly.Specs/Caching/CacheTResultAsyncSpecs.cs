namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CacheTResultAsyncSpecs : IDisposable
{
    #region Configuration

    private static CancellationToken CancellationToken => CancellationToken.None;

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;

        IAsyncCacheProvider<EmptyStruct> asyncCacheProvider = new StubCacheProvider().AsyncFor<EmptyStruct>();
        ITtlStrategy<EmptyStruct> ttlStrategy = new ContextualTtl().For<EmptyStruct>();
        Func<Context, string> cacheKeyStrategy = (_) => string.Empty;
        Action<Context, string> onCacheGet = (_, _) => { };
        Action<Context, string> onCacheMiss = (_, _) => { };
        Action<Context, string> onCachePut = (_, _) => { };
        Action<Context, string, Exception>? onCacheGetError = null;
        Action<Context, string, Exception>? onCachePutError = null;

        var instance = Activator.CreateInstance(
            typeof(AsyncCachePolicy<EmptyStruct>),
            flags,
            null,
            [
                asyncCacheProvider,
                ttlStrategy,
                cacheKeyStrategy,
                onCacheGet,
                onCacheMiss,
                onCachePut,
                onCacheGetError,
                onCachePutError,
            ],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.And.InnerException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("action");
    }

    [Fact]
    public void Should_throw_when_cache_provider_is_null()
    {
        IAsyncCacheProvider cacheProvider = null!;
        Action action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, TimeSpan.MaxValue);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");
    }

    [Fact]
    public void Should_throw_when_ttl_strategy_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        ITtlStrategy ttlStrategy = null!;
        Action action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, ttlStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ttlStrategy");
    }

    [Fact]
    public void Should_throw_when_cache_key_strategy_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = null!;
        Func<Context, string> cacheKeyStrategyFunc = null!;
        Action<Context, string> onCacheGet = (_, _) => { };
        Action<Context, string> onCacheMiss = (_, _) => { };
        Action<Context, string> onCachePut = (_, _) => { };
        Action<Context, string, Exception>? onCacheGetError = null;
        Action<Context, string, Exception>? onCachePutError = null;
        const string CacheKeyStrategyExpected = "cacheKeyStrategy";

        Action action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategy, onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategyFunc);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider.AsyncFor<ResultPrimitive>(), ttl, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(cacheProvider.AsyncFor<ResultPrimitive>(), ttlStrategy, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(
            cacheProvider.AsyncFor<ResultPrimitive>(),
            ttlStrategy.For<ResultPrimitive>(),
            cacheKeyStrategy,
            onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(
            cacheProvider.AsyncFor<ResultPrimitive>(),
            ttl,
            cacheKeyStrategy,
            onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(
            cacheProvider.AsyncFor<ResultPrimitive>(),
            ttl,
            cacheKeyStrategy,
            onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(
            cacheProvider.AsyncFor<ResultPrimitive>(),
            ttl,
            cacheKeyStrategy,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(
            cacheProvider.AsyncFor<ResultPrimitive>(),
            ttlStrategy,
            cacheKeyStrategy,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync<ResultPrimitive>(
            cacheProvider.AsyncFor<ResultPrimitive>(),
            ttlStrategy.For<ResultPrimitive>(),
            cacheKeyStrategy,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);
    }
    #endregion

    #region Caching behaviours

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken.None, false);

        bool delegateExecuted = false;

        (await cache.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey)))
            .Should().Be(ValueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return ValueToReturn; }, new Context(OperationKey))).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);
    }

    [Fact]
    public async Task Should_execute_delegate_and_put_value_in_cache_but_when_it_expires_execute_delegate_again()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        TimeSpan ttl = TimeSpan.FromMinutes(30);
        var cache = Policy.CacheAsync<string>(stubCacheProvider, ttl);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        int delegateInvocations = 0;
        Func<Context, Task<string>> func = async _ =>
        {
            delegateInvocations++;
            await TaskHelper.EmptyTask;
            return ValueToReturn;
        };

        DateTimeOffset fixedTime = SystemClock.DateTimeOffsetUtcNow();
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime;

        // First execution should execute delegate and put result in the cache.
        (await cache.ExecuteAsync(func, new Context(OperationKey))).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);

        // Second execution (before cache expires) should get it from the cache - no further delegate execution.
        // (Manipulate time so just prior cache expiry).
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(-1);
        (await cache.ExecuteAsync(func, new Context(OperationKey))).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        // Manipulate time to force cache expiry.
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(1);

        // Third execution (cache expired) should not get it from the cache - should cause further delegate execution.
        (await cache.ExecuteAsync(func, new Context(OperationKey))).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(2);
    }

    [Fact]
    public async Task Should_execute_delegate_but_not_put_value_in_cache_if_cache_does_not_hold_value_but_ttl_indicates_not_worth_caching()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.Zero);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return ValueToReturn; }, new Context(OperationKey))).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeFalse();
        fromCache2.Should().BeNull();
    }

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var cache = Policy.CacheAsync<string>(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;
        Func<Context, Task<string>> func = async _ =>
        {
            delegateInvocations++;
            await TaskHelper.EmptyTask;
            return ValueToReturn;
        };

        (await cache.ExecuteAsync(func, new Context(OperationKey))).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        (await cache.ExecuteAsync(func, new Context(OperationKey))).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        (await cache.ExecuteAsync(func, new Context(OperationKey))).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);
    }

    [Fact]
    public async Task Should_allow_custom_FuncCacheKeyStrategy()
    {
        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider, TimeSpan.MaxValue, context => context.OperationKey + context["id"]);

        object person1 = new ResultClass(ResultPrimitive.Good, "person1");
        await stubCacheProvider.PutAsync("person1", person1, new Ttl(TimeSpan.MaxValue), CancellationToken, false);
        object person2 = new ResultClass(ResultPrimitive.Good, "person2");
        await stubCacheProvider.PutAsync("person2", person2, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool funcExecuted = false;
        Func<Context, Task<ResultClass>> func = async _ => { funcExecuted = true; await TaskHelper.EmptyTask; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

        (await cache.ExecuteAsync(func, new Context("person", CreateDictionary("id", "1")))).Should().BeSameAs(person1);
        funcExecuted.Should().BeFalse();

        (await cache.ExecuteAsync(func, new Context("person", CreateDictionary("id", "2")))).Should().BeSameAs(person2);
        funcExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_allow_custom_ICacheKeyStrategy()
    {
        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);

        var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider.AsyncFor<ResultClass>(), new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

        object person1 = new ResultClass(ResultPrimitive.Good, "person1");
        await stubCacheProvider.PutAsync("person1", person1, new Ttl(TimeSpan.MaxValue), CancellationToken, false);
        object person2 = new ResultClass(ResultPrimitive.Good, "person2");
        await stubCacheProvider.PutAsync("person2", person2, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool funcExecuted = false;
        Func<Context, Task<ResultClass>> func = async _ => { funcExecuted = true; await TaskHelper.EmptyTask; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

        (await cache.ExecuteAsync(func, new Context("person", CreateDictionary("id", "1")))).Should().BeSameAs(person1);
        funcExecuted.Should().BeFalse();

        (await cache.ExecuteAsync(func, new Context("person", CreateDictionary("id", "2")))).Should().BeSameAs(person2);
        funcExecuted.Should().BeFalse();
    }

    #endregion

    #region Caching behaviours, default(TResult)

    [Fact]
    public async Task Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_reference_type()
    {
        ResultClass? valueToReturn = null;
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<ResultClass?>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return valueToReturn; }, new Context(OperationKey))).Should().Be(valueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(valueToReturn);
    }

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_reference_type()
    {
        ResultClass? valueToReturnFromCache = null;
        ResultClass valueToReturnFromExecution = new ResultClass(ResultPrimitive.Good);
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<ResultClass>(stubCacheProvider, TimeSpan.MaxValue);
        await stubCacheProvider.PutAsync(OperationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;

        (await cache.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return valueToReturnFromExecution;
        }, new Context(OperationKey)))
            .Should().Be(valueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_value_type()
    {
        ResultPrimitive valueToReturn = default;
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return valueToReturn; }, new Context(OperationKey))).Should().Be(valueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(valueToReturn);
    }

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_value_type()
    {
        ResultPrimitive valueToReturnFromCache = default;
        ResultPrimitive valueToReturnFromExecution = ResultPrimitive.Good;
        valueToReturnFromExecution.Should().NotBe(valueToReturnFromCache);
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);
        await stubCacheProvider.PutAsync(OperationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;

        (await cache.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return valueToReturnFromExecution;
        }, new Context(OperationKey)))
            .Should().Be(valueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    #endregion

    #region Generic CachePolicy in PolicyWrap

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
        var noop = Policy.NoOpAsync();
        var wrap = cache.WrapAsync(noop);

        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;

        (await wrap.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey)))
            .Should().Be(ValueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_innermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
        var noop = Policy.NoOpAsync();
        var wrap = noop.WrapAsync(cache);

        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;

        (await wrap.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey)))
            .Should().Be(ValueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_mid_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);
        var noop = Policy.NoOpAsync<string>();
        var wrap = Policy.WrapAsync(noop, cache, noop);

        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;

        (await wrap.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey)))
            .Should().Be(ValueToReturnFromCache);

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
        Func<Task<string>> func = async () =>
        {
            delegateInvocations++;
            await TaskHelper.EmptyTask;
            return valueToReturn;
        };

        (await cache.ExecuteAsync(func /*, no operation key */)).Should().Be(valueToReturn);
        delegateInvocations.Should().Be(1);

        (await cache.ExecuteAsync(func /*, no operation key */)).Should().Be(valueToReturn);
        delegateInvocations.Should().Be(2);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task Should_honour_cancellation_even_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var cache = Policy.CacheAsync<string>(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;

        using (var tokenSource = new CancellationTokenSource())
        {
            Func<Context, CancellationToken, Task<string>> func = async (_, _) =>
            {
                // delegate does not observe cancellation token; test is whether CacheEngine does.
                delegateInvocations++;
                await TaskHelper.EmptyTask;
                return ValueToReturn;
            };

            (await cache.ExecuteAsync(func, new Context(OperationKey), tokenSource.Token)).Should().Be(ValueToReturn);
            delegateInvocations.Should().Be(1);

            tokenSource.Cancel();

            await cache.Awaiting(policy => policy.ExecuteAsync(func, new Context(OperationKey), tokenSource.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        delegateInvocations.Should().Be(1);
    }

    [Fact]
    public async Task Should_honour_cancellation_during_delegate_execution_and_not_put_to_cache()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync<string>(stubCacheProvider, TimeSpan.MaxValue);

        using (var tokenSource = new CancellationTokenSource())
        {
            Func<Context, CancellationToken, Task<string>> func = async (_, ct) =>
            {
                tokenSource.Cancel(); // simulate cancellation raised during delegate execution
                ct.ThrowIfCancellationRequested();
                await TaskHelper.EmptyTask;
                return ValueToReturn;
            };

            await cache.Awaiting(policy => policy.ExecuteAsync(func, new Context(OperationKey), tokenSource.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit.Should().BeFalse();
        fromCache.Should().BeNull();
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
