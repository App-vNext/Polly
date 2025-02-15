namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CacheTResultSpecs : IDisposable
{
    #region Configuration

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;

        ISyncCacheProvider<EmptyStruct> syncCacheProvider = new StubCacheProvider().For<EmptyStruct>();
        ITtlStrategy<EmptyStruct> ttlStrategy = new ContextualTtl().For<EmptyStruct>();
        Func<Context, string> cacheKeyStrategy = (_) => string.Empty;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;

        var instance = Activator.CreateInstance(
            typeof(CachePolicy<EmptyStruct>),
            flags,
            null,
            [
                syncCacheProvider,
                ttlStrategy,
                cacheKeyStrategy,
                onCache,
                onCache,
                onCache,
                onCacheError,
                onCacheError,
            ],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void For_throws_if_cache_provider_is_null()
    {
        ISyncCacheProvider nonGenericCacheProvider = null!;
        Should.Throw<ArgumentNullException>(() => nonGenericCacheProvider.For<string>()).ParamName.ShouldBe("nonGenericCacheProvider");
    }

    [Fact]
    public void Should_throw_when_cache_provider_is_null()
    {
        ISyncCacheProvider cacheProvider = null!;
        ISyncCacheProvider<ResultPrimitive> cacheProviderGeneric = null!;
        var ttl = TimeSpan.MaxValue;
        var ttlStrategy = new ContextualTtl();
        var ttlStrategyGeneric = new ContextualTtl().For<ResultPrimitive>();
        var cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string CacheProviderExpected = "cacheProvider";

        Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttl, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttl, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttl, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttl, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheProviderExpected);
    }

    [Fact]
    public void Should_throw_when_ttl_strategy_is_null()
    {
        var cacheProvider = new StubCacheProvider();
        ISyncCacheProvider<ResultPrimitive> cacheProviderGeneric = new StubCacheProvider().For<ResultPrimitive>();
        ITtlStrategy ttlStrategy = null!;
        ITtlStrategy<ResultPrimitive> ttlStrategyGeneric = null!;
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string TtlStrategyExpected = "ttlStrategy";

        Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy, onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(TtlStrategyExpected);
    }

    [Fact]
    public void Should_throw_when_cache_key_strategy_is_null()
    {
        var cacheProvider = new StubCacheProvider();
        ISyncCacheProvider<ResultPrimitive> cacheProviderGeneric = new StubCacheProvider().For<ResultPrimitive>();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ITtlStrategy<ResultPrimitive> ttlStrategyGeneric = new ContextualTtl().For<ResultPrimitive>();
        ICacheKeyStrategy cacheKeyStrategy = null!;
        Func<Context, string> cacheKeyStrategyFunc = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string CacheKeyStrategyExpected = "cacheKeyStrategy";

        Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttl, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, cacheKeyStrategy, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttl, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategy, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(cacheProviderGeneric, ttlStrategyGeneric, cacheKeyStrategyFunc, onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(CacheKeyStrategyExpected);
    }

    [Fact]
    public void Should_throw_when_on_cache_get_is_null()
    {
        var cacheProvider = new StubCacheProvider();
        ISyncCacheProvider<ResultPrimitive> cacheProviderGeneric = new StubCacheProvider().For<ResultPrimitive>();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ITtlStrategy<ResultPrimitive> ttlStrategyGeneric = new ContextualTtl().For<ResultPrimitive>();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCacheGet = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string OnCacheGetExpected = "onCacheGet";

        Action action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategy,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategyFunc,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategyFunc,
            onCacheGet,
            onCache,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheGetExpected);
    }

    [Fact]
    public void Should_throw_when_on_cache_miss_is_null()
    {
        var cacheProvider = new StubCacheProvider();
        ISyncCacheProvider<ResultPrimitive> cacheProviderGeneric = new StubCacheProvider().For<ResultPrimitive>();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ITtlStrategy<ResultPrimitive> ttlStrategyGeneric = new ContextualTtl().For<ResultPrimitive>();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCacheMiss = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string OnCacheMissExpected = "onCacheMiss";

        Action action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategy,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategyFunc,
            onCache,
            onCacheMiss,
            onCache,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCacheMissExpected);
    }

    [Fact]
    public void Should_throw_when_on_cache_put_is_null()
    {
        var cacheProvider = new StubCacheProvider();
        ISyncCacheProvider<ResultPrimitive> cacheProviderGeneric = new StubCacheProvider().For<ResultPrimitive>();
        var ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ITtlStrategy<ResultPrimitive> ttlStrategyGeneric = new ContextualTtl().For<ResultPrimitive>();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = (_) => string.Empty;
        Action<Context, string> onCachePut = null!;
        Action<Context, string> onCache = (_, _) => { };
        Action<Context, string, Exception>? onCacheError = null;
        const string OnCachePutExpected = "onCachePut";

        Action action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategy,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttl,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategy,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);

        action = () => Policy.Cache(
            cacheProviderGeneric,
            ttlStrategyGeneric,
            cacheKeyStrategyFunc,
            onCache,
            onCache,
            onCachePut,
            onCacheError,
            onCacheError);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe(OnCachePutExpected);
    }

    #endregion

    #region Caching behaviours

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        cache.Execute(_ =>
        {
            delegateExecuted = true;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey))
            .ShouldBe(ValueToReturnFromCache);

        delegateExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.ShouldBeFalse();
        fromCache1.ShouldBeNull();

        cache.Execute(_ => ValueToReturn, new Context(OperationKey)).ShouldBe(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.ShouldBeTrue();
        fromCache2.ShouldBe(ValueToReturn);
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_but_when_it_expires_execute_delegate_again()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        var ttl = TimeSpan.FromMinutes(30);
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, ttl);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.ShouldBeFalse();
        fromCache1.ShouldBeNull();

        int delegateInvocations = 0;
        Func<Context, string> func = _ =>
        {
            delegateInvocations++;
            return ValueToReturn;
        };

        DateTimeOffset fixedTime = SystemClock.DateTimeOffsetUtcNow();
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime;

        // First execution should execute delegate and put result in the cache.
        cache.Execute(func, new Context(OperationKey)).ShouldBe(ValueToReturn);
        delegateInvocations.ShouldBe(1);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.ShouldBeTrue();
        fromCache2.ShouldBe(ValueToReturn);

        // Second execution (before cache expires) should get it from the cache - no further delegate execution.
        // (Manipulate time so just prior cache expiry).
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(-1);
        cache.Execute(func, new Context(OperationKey)).ShouldBe(ValueToReturn);
        delegateInvocations.ShouldBe(1);

        // Manipulate time to force cache expiry.
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(1);

        // Third execution (cache expired) should not get it from the cache - should cause further delegate execution.
        cache.Execute(func, new Context(OperationKey)).ShouldBe(ValueToReturn);
        delegateInvocations.ShouldBe(2);
    }

    [Fact]
    public void Should_execute_delegate_but_not_put_value_in_cache_if_cache_does_not_hold_value_but_ttl_indicates_not_worth_caching()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.Zero);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.ShouldBeFalse();
        fromCache1.ShouldBeNull();

        cache.Execute(_ => ValueToReturn, new Context(OperationKey)).ShouldBe(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.ShouldBeFalse();
        fromCache2.ShouldBeNull();
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        CachePolicy<string> cache = Policy.Cache<string>(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;
        Func<Context, string> func = _ =>
        {
            delegateInvocations++;
            return ValueToReturn;
        };

        cache.Execute(func, new Context(OperationKey)).ShouldBe(ValueToReturn);
        delegateInvocations.ShouldBe(1);

        cache.Execute(func, new Context(OperationKey)).ShouldBe(ValueToReturn);
        delegateInvocations.ShouldBe(1);

        cache.Execute(func, new Context(OperationKey)).ShouldBe(ValueToReturn);
        delegateInvocations.ShouldBe(1);
    }

    [Fact]
    public void Should_allow_custom_FuncICacheKeyStrategy()
    {
        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider, TimeSpan.MaxValue, context => context.OperationKey + context["id"]);

        object person1 = new ResultClass(ResultPrimitive.Good, "person1");
        stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
        object person2 = new ResultClass(ResultPrimitive.Good, "person2");
        stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

        bool funcExecuted = false;
        Func<Context, ResultClass> func = _ => { funcExecuted = true; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

        cache.Execute(func, new Context("person", CreateDictionary("id", "1"))).ShouldBeSameAs(person1);
        funcExecuted.ShouldBeFalse();

        cache.Execute(func, new Context("person", CreateDictionary("id", "2"))).ShouldBeSameAs(person2);
        funcExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_allow_custom_ICacheKeyStrategy()
    {
        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };

        var stubCacheProvider = new StubCacheProvider();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        CachePolicy<ResultClass> cache = Policy.Cache(stubCacheProvider.For<ResultClass>(), new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

        object person1 = new ResultClass(ResultPrimitive.Good, "person1");
        stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
        object person2 = new ResultClass(ResultPrimitive.Good, "person2");
        stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

        bool funcExecuted = false;
        Func<Context, ResultClass> func = _ => { funcExecuted = true; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

        cache.Execute(func, new Context("person", CreateDictionary("id", "1"))).ShouldBeSameAs(person1);
        funcExecuted.ShouldBeFalse();

        cache.Execute(func, new Context("person", CreateDictionary("id", "2"))).ShouldBeSameAs(person2);
        funcExecuted.ShouldBeFalse();
    }

    #endregion

    #region Caching behaviours, default(TResult)

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_reference_type()
    {
        ResultClass? valueToReturn = null;
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultClass?> cache = Policy.Cache<ResultClass?>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.ShouldBeFalse();
        fromCache1.ShouldBeNull();

        cache.Execute(_ => valueToReturn, new Context(OperationKey)).ShouldBe(valueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.ShouldBeTrue();
        fromCache2.ShouldBe(valueToReturn);
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_reference_type()
    {
        ResultClass? valueToReturnFromCache = null;
        ResultClass valueToReturnFromExecution = new ResultClass(ResultPrimitive.Good);
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider, TimeSpan.MaxValue);
        stubCacheProvider.Put(OperationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        cache.Execute(_ =>
        {
            delegateExecuted = true;
            return valueToReturnFromExecution;
        }, new Context(OperationKey))
            .ShouldBe(valueToReturnFromCache);

        delegateExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_value_type()
    {
        ResultPrimitive valueToReturn = default;
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultPrimitive> cache = Policy.Cache<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.ShouldBeFalse();
        fromCache1.ShouldBeNull();

        cache.Execute(_ => valueToReturn, new Context(OperationKey)).ShouldBe(valueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.ShouldBeTrue();
        fromCache2.ShouldBe(valueToReturn);
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_value_type()
    {
        ResultPrimitive valueToReturnFromCache = default;
        ResultPrimitive valueToReturnFromExecution = ResultPrimitive.Good;
        valueToReturnFromExecution.ShouldNotBe(valueToReturnFromCache);
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultPrimitive> cache = Policy.Cache<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);
        stubCacheProvider.Put(OperationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        cache.Execute(_ =>
        {
            delegateExecuted = true;
            return valueToReturnFromExecution;
        }, new Context(OperationKey))
            .ShouldBe(valueToReturnFromCache);

        delegateExecuted.ShouldBeFalse();
    }

    #endregion

    #region Generic CachePolicy in PolicyWrap

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
        Policy noop = Policy.NoOp();
        PolicyWrap<string> wrap = cache.Wrap(noop);

        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        wrap.Execute(_ =>
        {
            delegateExecuted = true;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey))
            .ShouldBe(ValueToReturnFromCache);

        delegateExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_innermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
        Policy noop = Policy.NoOp();
        PolicyWrap<string> wrap = noop.Wrap(cache);

        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        wrap.Execute(_ =>
        {
            delegateExecuted = true;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey))
            .ShouldBe(ValueToReturnFromCache);

        delegateExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_mid_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
        Policy<string> noop = Policy.NoOp<string>();
        PolicyWrap<string> wrap = Policy.Wrap(noop, cache, noop);

        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        wrap.Execute(_ =>
        {
            delegateExecuted = true;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey))
            .ShouldBe(ValueToReturnFromCache);

        delegateExecuted.ShouldBeFalse();
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

        cache.Execute(func /*, no operation key */).ShouldBe(valueToReturn);
        delegateInvocations.ShouldBe(1);

        cache.Execute(func /*, no operation key */).ShouldBe(valueToReturn);
        delegateInvocations.ShouldBe(2);
    }

    #endregion

    #region Cancellation

    [Fact]
    public void Should_honour_cancellation_even_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        CachePolicy<string> policy = Policy.Cache<string>(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;

        using (var tokenSource = new CancellationTokenSource())
        {
            Func<Context, CancellationToken, string> func = (_, _) =>
            {
                // delegate does not observe cancellation token; test is whether CacheEngine does.
                delegateInvocations++;
                return ValueToReturn;
            };

            policy.Execute(func, new Context(OperationKey), tokenSource.Token).ShouldBe(ValueToReturn);
            delegateInvocations.ShouldBe(1);

            tokenSource.Cancel();

            Should.Throw<OperationCanceledException>(() => policy.Execute(func, new Context(OperationKey), tokenSource.Token));
        }

        delegateInvocations.ShouldBe(1);
    }

    [Fact]
    public void Should_honour_cancellation_during_delegate_execution_and_not_put_to_cache()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        var stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> policy = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);

        using (var tokenSource = new CancellationTokenSource())
        {
            Func<Context, CancellationToken, string> func = (_, ct) =>
            {
                tokenSource.Cancel(); // simulate cancellation raised during delegate execution
                ct.ThrowIfCancellationRequested();
                return ValueToReturn;
            };

            Should.Throw<OperationCanceledException>(() => policy.Execute(func, new Context(OperationKey), tokenSource.Token));
        }

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(OperationKey);
        cacheHit.ShouldBeFalse();
        fromCache.ShouldBeNull();
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
