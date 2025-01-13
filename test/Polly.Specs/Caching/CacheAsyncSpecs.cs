namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CacheAsyncSpecs : IDisposable
{
    #region Configuration

    private static CancellationToken CancellationToken => CancellationToken.None;

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        Func<Context, CancellationToken, Task> actionVoid = null!;

        IAsyncCacheProvider asyncCacheProvider = new StubCacheProvider();
        ITtlStrategy ttlStrategy = new ContextualTtl();
        Func<Context, string> cacheKeyStrategy = (_) => string.Empty;
        Action<Context, string> onCacheGet = (_, _) => { };
        Action<Context, string> onCacheMiss = (_, _) => { };
        Action<Context, string> onCachePut = (_, _) => { };
        Action<Context, string, Exception>? onCacheGetError = null;
        Action<Context, string, Exception>? onCachePutError = null;

        var instance = Activator.CreateInstance(
            typeof(AsyncCachePolicy),
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
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.And.InnerException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("action");

        methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task" });

        func = () => methodInfo.Invoke(instance, [actionVoid, new Context(), CancellationToken, false]);

        exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.And.InnerException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("action");
    }

    [Fact]
    public void Should_throw_when_cache_provider_is_null()
    {
        IAsyncCacheProvider cacheProvider = null!;
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string CacheProviderExpected = "cacheProvider";

        Action action = () => Policy.CacheAsync(cacheProvider, ttl, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategyFunc, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategy, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategyFunc, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheProviderExpected);
    }

    [Fact]
    public void Should_throw_when_ttl_strategy_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        ITtlStrategy ttlStrategy = null!;
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string TtlStrategyExpected = "ttlStrategy";

        Action action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(TtlStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(TtlStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(TtlStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(TtlStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(TtlStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCache, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(TtlStrategyExpected);
    }

    [Fact]
    public void Should_throw_when_cache_key_strategy_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = null!;
        Func<Context, string> cacheKeyStrategyFunc = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string CacheKeyStrategyExpected = "cacheKeyStrategy";

        Action action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategyFunc, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(
            cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.CacheAsync(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);
    }

    [Fact]
    public void Should_throw_when_on_cache_get_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCacheGet = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string OnCacheGetExpected = "onCacheGet";

        Action action = () => Policy.CacheAsync(cacheProvider, ttl, onCacheGet, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheGetExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCacheGet, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheGetExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategy, onCacheGet, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheGetExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheGet, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheGetExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategyFunc, onCacheGet, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheGetExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheGet, onCache, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheGetExpected);
    }

    [Fact]
    public void Should_throw_when_on_cache_miss_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCacheMiss = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string OnCacheMissExpected = "onCacheMiss";

        Action action = () => Policy.CacheAsync(cacheProvider, ttl, onCache, onCacheMiss, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheMissExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCache, onCacheMiss, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheMissExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategy, onCache, onCacheMiss, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheMissExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCache, onCacheMiss, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheMissExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategyFunc, onCache, onCacheMiss, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheMissExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCache, onCacheMiss, onCache, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCacheMissExpected);
    }

    [Fact]
    public void Should_throw_when_on_cache_put_is_null()
    {
        IAsyncCacheProvider cacheProvider = new StubCacheProvider();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCachePut = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string OnCachePutExpected = "onCachePut";

        Action action = () => Policy.CacheAsync(cacheProvider, ttl, onCache, onCache, onCachePut, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCachePutExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, onCache, onCache, onCachePut, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCachePutExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategy, onCache, onCache, onCachePut, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCachePutExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy, onCache, onCache, onCachePut, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCachePutExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttl, cacheKeyStrategyFunc, onCache, onCache, onCachePut, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCachePutExpected);

        action = () => Policy.CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCache, onCache, onCachePut, onCacheError, onCacheError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(OnCachePutExpected);
    }

    [Fact]
    public void Should_throw_when_policies_is_null()
    {
        IAsyncPolicy[] policies = null!;
        IAsyncPolicy<int>[] policiesGeneric = null!;

        Action action = () => Policy.WrapAsync(policies);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("policies");

        action = () => Policy.WrapAsync<int>(policiesGeneric);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("policies");
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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

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
        var cache = Policy.CacheAsync(stubCacheProvider, ttl);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.Zero);

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

        var cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, context => context.OperationKey + context["id"]);

        object person1 = new();
        await stubCacheProvider.PutAsync("person1", person1, new Ttl(TimeSpan.MaxValue), CancellationToken, false);
        object person2 = new();
        await stubCacheProvider.PutAsync("person2", person2, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool funcExecuted = false;
        Func<Context, Task<object>> func = async _ => { funcExecuted = true; await TaskHelper.EmptyTask; return new object(); };

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
        var cache = Policy.CacheAsync(stubCacheProvider, new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

        object person1 = new();
        await stubCacheProvider.PutAsync("person1", person1, new Ttl(TimeSpan.MaxValue), CancellationToken, false);
        object person2 = new();
        await stubCacheProvider.PutAsync("person2", person2, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool funcExecuted = false;
        Func<Context, Task<object>> func = async _ => { funcExecuted = true; await TaskHelper.EmptyTask; return new object(); };

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
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

    #region Non-generic CachePolicy in non-generic PolicyWrap

    [Fact]
    public async Task Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
        var noop = Policy.NoOpAsync();
        var wrap = Policy.WrapAsync(cache, noop);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
        var noop = Policy.NoOpAsync();
        var wrap = Policy.WrapAsync(noop, cache);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);
        var noop = Policy.NoOpAsync();
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

        var cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

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

    [Fact]
    public void Should_always_execute_delegate_if_execution_is_void_returning()
    {
        string operationKey = "SomeKey";

        var cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;
        Func<Context, Task> action = async _ => { delegateInvocations++; await TaskHelper.EmptyTask; };

        cache.ExecuteAsync(action, new Context(operationKey));
        delegateInvocations.Should().Be(1);

        cache.ExecuteAsync(action, new Context(operationKey));
        delegateInvocations.Should().Be(2);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task Should_honour_cancellation_even_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var cache = Policy.CacheAsync(new StubCacheProvider(), TimeSpan.MaxValue);

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
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

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

    #region Policy hooks

    [Fact]
    public async Task Should_call_onError_delegate_if_cache_get_errors()
    {
        Exception ex = new Exception();
        IAsyncCacheProvider stubCacheProvider = new StubErroringCacheProvider(getException: ex, putException: null);

        Exception? exceptionFromCacheProvider = null;

        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        Action<Context, string, Exception> onError = (_, _, exc) => { exceptionFromCacheProvider = exc; };

        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, onError);

        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;

        // Even though value is in cache, get will error; so value is returned from execution.
        (await cache.ExecuteAsync(async _ =>
        {
            delegateExecuted = true;
            await TaskHelper.EmptyTask;
            return ValueToReturnFromExecution;

        }, new Context(OperationKey)))
           .Should().Be(ValueToReturnFromExecution);
        delegateExecuted.Should().BeTrue();

        // And error should be captured by onError delegate.
        exceptionFromCacheProvider.Should().Be(ex);
    }

    [Fact]
    public async Task Should_call_onError_delegate_if_cache_put_errors()
    {
        Exception ex = new Exception();
        IAsyncCacheProvider stubCacheProvider = new StubErroringCacheProvider(getException: null, putException: ex);

        Exception? exceptionFromCacheProvider = null;

        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        Action<Context, string, Exception> onError = (_, _, exc) => { exceptionFromCacheProvider = exc; };

        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, onError);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return ValueToReturn; }, new Context(OperationKey))).Should().Be(ValueToReturn);

        // error should be captured by onError delegate.
        exceptionFromCacheProvider.Should().Be(ex);

        // failed to put it in the cache
        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeFalse();
        fromCache2.Should().BeNull();
    }

    [Fact]
    public async Task Should_execute_oncacheget_after_got_from_cache()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";

        const string OperationKey = "SomeOperationKey";
        string? keyPassedToDelegate = null;

        Context contextToExecute = new Context(OperationKey);
        Context? contextPassedToDelegate = null;

        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };
        Action<Context, string> onCacheAction = (ctx, key) => { contextPassedToDelegate = ctx; keyPassedToDelegate = key; };

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, new RelativeTtl(TimeSpan.MaxValue), DefaultCacheKeyStrategy.Instance, onCacheAction, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);
        await stubCacheProvider.PutAsync(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue), CancellationToken, false);

        bool delegateExecuted = false;
        (await cache.ExecuteAsync(async _ =>
                {
                    delegateExecuted = true;
                    await TaskHelper.EmptyTask;
                    return ValueToReturnFromExecution;
                }, contextToExecute))
            .Should().Be(ValueToReturnFromCache);
        delegateExecuted.Should().BeFalse();

        contextPassedToDelegate.Should().BeSameAs(contextToExecute);
        keyPassedToDelegate.Should().Be(OperationKey);
    }

    [Fact]
    public async Task Should_execute_oncachemiss_and_oncacheput_if_cache_does_not_hold_value_and_put()
    {
        const string ValueToReturn = "valueToReturn";

        const string OperationKey = "SomeOperationKey";
        string? keyPassedToOnCacheMiss = null;
        string? keyPassedToOnCachePut = null;

        Context contextToExecute = new Context(OperationKey);
        Context? contextPassedToOnCacheMiss = null;
        Context? contextPassedToOnCachePut = null;

        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };
        Action<Context, string> onCacheMiss = (ctx, key) => { contextPassedToOnCacheMiss = ctx; keyPassedToOnCacheMiss = key; };
        Action<Context, string> onCachePut = (ctx, key) => { contextPassedToOnCachePut = ctx; keyPassedToOnCachePut = key; };

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, new RelativeTtl(TimeSpan.MaxValue), DefaultCacheKeyStrategy.Instance, emptyDelegate, onCacheMiss, onCachePut, noErrorHandling, noErrorHandling);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return ValueToReturn; }, contextToExecute)).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);

        contextPassedToOnCachePut.Should().BeSameAs(contextToExecute);
        keyPassedToOnCachePut.Should().Be(OperationKey);
        contextPassedToOnCacheMiss.Should().NotBeNull();
        keyPassedToOnCacheMiss.Should().Be("SomeOperationKey");
    }

    [Fact]
    public async Task Should_execute_oncachemiss_but_not_oncacheput_if_cache_does_not_hold_value_and_returned_value_not_worth_caching()
    {
        const string ValueToReturn = "valueToReturn";

        const string OperationKey = "SomeOperationKey";
        string? keyPassedToOnCacheMiss = null;
        string? keyPassedToOnCachePut = null;

        Context contextToExecute = new Context(OperationKey);
        Context? contextPassedToOnCacheMiss = null;
        Context? contextPassedToOnCachePut = null;

        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };
        Action<Context, string> onCacheMiss = (ctx, key) => { contextPassedToOnCacheMiss = ctx; keyPassedToOnCacheMiss = key; };
        Action<Context, string> onCachePut = (ctx, key) => { contextPassedToOnCachePut = ctx; keyPassedToOnCachePut = key; };

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, new RelativeTtl(TimeSpan.Zero), DefaultCacheKeyStrategy.Instance, emptyDelegate, onCacheMiss, onCachePut, noErrorHandling, noErrorHandling);

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken, false);
        cacheHit.Should().BeFalse();
        fromCache.Should().BeNull();

        (await cache.ExecuteAsync(async _ => { await TaskHelper.EmptyTask; return ValueToReturn; }, contextToExecute)).Should().Be(ValueToReturn);

        contextPassedToOnCachePut.Should().BeNull();
        keyPassedToOnCachePut.Should().BeNull();
        contextPassedToOnCacheMiss.Should().BeEmpty();
        keyPassedToOnCacheMiss.Should().Be("SomeOperationKey");
    }

    [Fact]
    public async Task Should_not_execute_oncachemiss_if_dont_query_cache_because_cache_key_not_set()
    {
        string valueToReturn = Guid.NewGuid().ToString();

        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };

        bool onCacheMissExecuted = false;
        Action<Context, string> onCacheMiss = (_, _) => { onCacheMissExecuted = true; };

        var cache = Policy.CacheAsync(new StubCacheProvider(), new RelativeTtl(TimeSpan.MaxValue), DefaultCacheKeyStrategy.Instance, emptyDelegate, onCacheMiss, emptyDelegate, noErrorHandling, noErrorHandling);

        (await cache.ExecuteAsync(async () =>
        {
            await TaskHelper.EmptyTask;
            return valueToReturn;
        }  /*, no operation key */))
        .Should().Be(valueToReturn);

        onCacheMissExecuted.Should().BeFalse();
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
