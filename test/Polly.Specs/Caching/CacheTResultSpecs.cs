namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CacheTResultSpecs : IDisposable
{
    #region Configuration

    [Fact]
    public void Should_throw_when_cache_provider_is_null()
    {
        ISyncCacheProvider cacheProvider = null!;
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);

        Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, TimeSpan.MaxValue);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, new ContextualTtl(), cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cacheProvider");
    }

    [Fact]
    public void Should_throw_when_ttl_strategy_is_null()
    {
        ISyncCacheProvider cacheProvider = new StubCacheProvider();
        ITtlStrategy ttlStrategy = null!;
        Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ttlStrategy");
    }

    [Fact]
    public void Should_throw_when_cache_key_strategy_is_null()
    {
        ISyncCacheProvider cacheProvider = new StubCacheProvider();
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

        Action action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider, ttl, cacheKeyStrategyFunc);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider.For<ResultPrimitive>(), ttl, cacheKeyStrategy, onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(cacheProvider.For<ResultPrimitive>(), ttlStrategy, cacheKeyStrategy, onCacheGetError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider.For<ResultPrimitive>(),
            ttl,
            cacheKeyStrategy,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider.For<ResultPrimitive>(),
            ttlStrategy,
            cacheKeyStrategy,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(CacheKeyStrategyExpected);

        action = () => Policy.Cache<ResultPrimitive>(
            cacheProvider.For<ResultPrimitive>(),
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
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);
        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        cache.Execute(_ =>
        {
            delegateExecuted = true;
            return ValueToReturnFromExecution;
        }, new Context(OperationKey))
            .Should().Be(ValueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        cache.Execute(_ => ValueToReturn, new Context(OperationKey)).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_but_when_it_expires_execute_delegate_again()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        TimeSpan ttl = TimeSpan.FromMinutes(30);
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, ttl);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        int delegateInvocations = 0;
        Func<Context, string> func = _ =>
        {
            delegateInvocations++;
            return ValueToReturn;
        };

        DateTimeOffset fixedTime = SystemClock.DateTimeOffsetUtcNow();
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime;

        // First execution should execute delegate and put result in the cache.
        cache.Execute(func, new Context(OperationKey)).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);

        // Second execution (before cache expires) should get it from the cache - no further delegate execution.
        // (Manipulate time so just prior cache expiry).
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(-1);
        cache.Execute(func, new Context(OperationKey)).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        // Manipulate time to force cache expiry.
        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(ttl).AddSeconds(1);

        // Third execution (cache expired) should not get it from the cache - should cause further delegate execution.
        cache.Execute(func, new Context(OperationKey)).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(2);
    }

    [Fact]
    public void Should_execute_delegate_but_not_put_value_in_cache_if_cache_does_not_hold_value_but_ttl_indicates_not_worth_caching()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.Zero);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        cache.Execute(_ => ValueToReturn, new Context(OperationKey)).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeFalse();
        fromCache2.Should().BeNull();
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

        cache.Execute(func, new Context(OperationKey)).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        cache.Execute(func, new Context(OperationKey)).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);

        cache.Execute(func, new Context(OperationKey)).Should().Be(ValueToReturn);
        delegateInvocations.Should().Be(1);
    }

    [Fact]
    public void Should_allow_custom_FuncICacheKeyStrategy()
    {
        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider, TimeSpan.MaxValue, context => context.OperationKey + context["id"]);

        object person1 = new ResultClass(ResultPrimitive.Good, "person1");
        stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
        object person2 = new ResultClass(ResultPrimitive.Good, "person2");
        stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

        bool funcExecuted = false;
        Func<Context, ResultClass> func = _ => { funcExecuted = true; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

        cache.Execute(func, new Context("person", new { id = "1" }.AsDictionary())).Should().BeSameAs(person1);
        funcExecuted.Should().BeFalse();

        cache.Execute(func, new Context("person", new { id = "2" }.AsDictionary())).Should().BeSameAs(person2);
        funcExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_allow_custom_ICacheKeyStrategy()
    {
        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider.For<ResultClass>(), new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

        object person1 = new ResultClass(ResultPrimitive.Good, "person1");
        stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
        object person2 = new ResultClass(ResultPrimitive.Good, "person2");
        stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

        bool funcExecuted = false;
        Func<Context, ResultClass> func = _ => { funcExecuted = true; return new ResultClass(ResultPrimitive.Fault, "should never return this one"); };

        cache.Execute(func, new Context("person", new { id = "1" }.AsDictionary())).Should().BeSameAs(person1);
        funcExecuted.Should().BeFalse();

        cache.Execute(func, new Context("person", new { id = "2" }.AsDictionary())).Should().BeSameAs(person2);
        funcExecuted.Should().BeFalse();
    }

    #endregion

    #region Caching behaviours, default(TResult)

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_reference_type()
    {
        ResultClass? valueToReturn = null;
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultClass?> cache = Policy.Cache<ResultClass?>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        cache.Execute(_ => valueToReturn, new Context(OperationKey)).Should().Be(valueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(valueToReturn);
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_reference_type()
    {
        ResultClass? valueToReturnFromCache = null;
        ResultClass valueToReturnFromExecution = new ResultClass(ResultPrimitive.Good);
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultClass> cache = Policy.Cache<ResultClass>(stubCacheProvider, TimeSpan.MaxValue);
        stubCacheProvider.Put(OperationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        cache.Execute(_ =>
        {
            delegateExecuted = true;
            return valueToReturnFromExecution;
        }, new Context(OperationKey))
            .Should().Be(valueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_if_cache_does_not_hold_value__default_for_value_type()
    {
        ResultPrimitive valueToReturn = default;
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultPrimitive> cache = Policy.Cache<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        cache.Execute(_ => valueToReturn, new Context(OperationKey)).Should().Be(valueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(valueToReturn);
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value__default_for_value_type()
    {
        ResultPrimitive valueToReturnFromCache = default;
        ResultPrimitive valueToReturnFromExecution = ResultPrimitive.Good;
        valueToReturnFromExecution.Should().NotBe(valueToReturnFromCache);
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<ResultPrimitive> cache = Policy.Cache<ResultPrimitive>(stubCacheProvider, TimeSpan.MaxValue);
        stubCacheProvider.Put(OperationKey, valueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        cache.Execute(_ =>
        {
            delegateExecuted = true;
            return valueToReturnFromExecution;
        }, new Context(OperationKey))
            .Should().Be(valueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    #endregion

    #region Generic CachePolicy in PolicyWrap

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
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
            .Should().Be(ValueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_innermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
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
            .Should().Be(ValueToReturnFromCache);

        delegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_mid_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
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
            .Should().Be(ValueToReturnFromCache);

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

        cache.Execute(func /*, no operation key */).Should().Be(valueToReturn);
        delegateInvocations.Should().Be(1);

        cache.Execute(func /*, no operation key */).Should().Be(valueToReturn);
        delegateInvocations.Should().Be(2);
    }

    #endregion

    #region Cancellation

    [Fact]
    public void Should_honour_cancellation_even_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        CachePolicy<string> cache = Policy.Cache<string>(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;

        using (var tokenSource = new CancellationTokenSource())
        {
            Func<Context, CancellationToken, string> func = (_, _) =>
            {
                // delegate does not observe cancellation token; test is whether CacheEngine does.
                delegateInvocations++;
                return ValueToReturn;
            };

            cache.Execute(func, new Context(OperationKey), tokenSource.Token).Should().Be(ValueToReturn);
            delegateInvocations.Should().Be(1);

            tokenSource.Cancel();

            cache.Invoking(policy => policy.Execute(func, new Context(OperationKey), tokenSource.Token))
                .Should().Throw<OperationCanceledException>();
        }

        delegateInvocations.Should().Be(1);
    }

    [Fact]
    public void Should_honour_cancellation_during_delegate_execution_and_not_put_to_cache()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy<string> cache = Policy.Cache<string>(stubCacheProvider, TimeSpan.MaxValue);

        using (var tokenSource = new CancellationTokenSource())
        {
            Func<Context, CancellationToken, string> func = (_, ct) =>
            {
                tokenSource.Cancel(); // simulate cancellation raised during delegate execution
                ct.ThrowIfCancellationRequested();
                return ValueToReturn;
            };

            cache.Invoking(policy => policy.Execute(func, new Context(OperationKey), tokenSource.Token))
                .Should().Throw<OperationCanceledException>();
        }

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(OperationKey);
        cacheHit.Should().BeFalse();
        fromCache.Should().BeNull();
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
