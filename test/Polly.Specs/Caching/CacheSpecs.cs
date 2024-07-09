using System;

namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CacheSpecs : IDisposable
{
    #region Configuration

    [Fact]
    public void Should_throw_when_cache_provider_is_null()
    {
        ISyncCacheProvider cacheProvider = null!;
        TimeSpan ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = _ => string.Empty;
        Action<Context, string> emptyDelegate = (_, _) => { };
        const string Expected = "cacheProvider";

        Action action = () => Policy.Cache(cacheProvider, ttl);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttl, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttlStrategy, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttlStrategy, cacheKeyStrategyFunc);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);
    }

    [Fact]
    public void Should_throw_when_ttl_strategy_is_null()
    {
        ISyncCacheProvider cacheProvider = new StubCacheProvider();
        ITtlStrategy ttlStrategy = null!;
        ICacheKeyStrategy cacheKeyStrategy = new StubCacheKeyStrategy(context => context.OperationKey + context["id"]);
        Func<Context, string> cacheKeyStrategyFunc = _ => string.Empty;
        Action<Context, string> emptyDelegate = (_, _) => { };
        const string Expected = "ttlStrategy";

        Action action = () => Policy.Cache(cacheProvider, ttlStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttlStrategy, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttlStrategy, cacheKeyStrategyFunc);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);
    }

    [Fact]
    public void Should_throw_when_cache_key_strategy_is_null()
    {
        ISyncCacheProvider cacheProvider = new StubCacheProvider();
        TimeSpan ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        ICacheKeyStrategy cacheKeyStrategy = null!;
        Func<Context, string> cacheKeyStrategyFunc = null!;
        Action<Context, string> emptyDelegate = (_, _) => { };
        const string Expected = "cacheKeyStrategy";

        Action action = () => Policy.Cache(cacheProvider, ttl, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttlStrategy, cacheKeyStrategy);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttl, cacheKeyStrategyFunc);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(cacheProvider, ttlStrategy, cacheKeyStrategyFunc);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(
            cacheProvider,
            ttl,
            cacheKeyStrategy,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(
            cacheProvider,
            ttl,
            cacheKeyStrategyFunc,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);

        action = () => Policy.Cache(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategyFunc,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(Expected);
    }

    [Fact]
    public void Should_return_cache_policy_when_cache_strategy_is_default()
    {
        ISyncCacheProvider cacheProvider = new StubCacheProvider();
        TimeSpan ttl = TimeSpan.MaxValue;
        ITtlStrategy ttlStrategy = new ContextualTtl();
        Action<Context, string> emptyDelegate = (_, _) => { };

        var policy = Policy.Cache(
            cacheProvider,
            ttl,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        policy.Should().NotBeNull();

        policy = Policy.Cache(
            cacheProvider,
            ttlStrategy,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            null,
            null);
        policy.Should().NotBeNull();
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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);
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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, ttl);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.Zero);

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

        CachePolicy cache = Policy.Cache(new StubCacheProvider(), TimeSpan.MaxValue);

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
    public void Should_allow_custom_FuncCacheKeyStrategy()
    {
        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue, context => context.OperationKey + context["id"]);

        object person1 = new();
        stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
        object person2 = new();
        stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

        bool funcExecuted = false;
        Func<Context, object> func = _ => { funcExecuted = true; return new object(); };

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, new RelativeTtl(TimeSpan.MaxValue), cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);

        object person1 = new();
        stubCacheProvider.Put("person1", person1, new Ttl(TimeSpan.MaxValue));
        object person2 = new();
        stubCacheProvider.Put("person2", person2, new Ttl(TimeSpan.MaxValue));

        bool funcExecuted = false;
        Func<Context, object> func = _ => { funcExecuted = true; return new object(); };

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);
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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);
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

    #region Non-generic CachePolicy in non-generic PolicyWrap

    [Fact]
    public void Should_return_value_from_cache_and_not_execute_delegate_if_cache_holds_value_when_outermost_in_policywrap()
    {
        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);
        Policy noop = Policy.NoOp();
        PolicyWrap wrap = Policy.Wrap(cache, noop);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);
        Policy noop = Policy.NoOp();
        PolicyWrap wrap = Policy.Wrap(noop, cache);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);
        Policy noop = Policy.NoOp();
        PolicyWrap wrap = Policy.Wrap(noop, cache, noop);

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

        CachePolicy cache = Policy.Cache(new StubCacheProvider(), TimeSpan.MaxValue);

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

    [Fact]
    public void Should_always_execute_delegate_if_execution_is_void_returning()
    {
        string operationKey = "SomeKey";

        CachePolicy cache = Policy.Cache(new StubCacheProvider(), TimeSpan.MaxValue);

        int delegateInvocations = 0;
        Action<Context> action = _ => { delegateInvocations++; };

        cache.Execute(action, new Context(operationKey));
        delegateInvocations.Should().Be(1);

        cache.Execute(action, new Context(operationKey));
        delegateInvocations.Should().Be(2);
    }

    #endregion

    #region Cancellation

    [Fact]
    public void Should_honour_cancellation_even_if_prior_execution_has_cached()
    {
        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        CachePolicy cache = Policy.Cache(new StubCacheProvider(), TimeSpan.MaxValue);

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
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue);

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

    #region Policy hooks

    [Fact]
    public void Should_call_onError_delegate_if_cache_get_errors()
    {
        Exception ex = new Exception();
        ISyncCacheProvider stubCacheProvider = new StubErroringCacheProvider(getException: ex, putException: null);

        Exception? exceptionFromCacheProvider = null;

        const string ValueToReturnFromCache = "valueToReturnFromCache";
        const string ValueToReturnFromExecution = "valueToReturnFromExecution";
        const string OperationKey = "SomeOperationKey";

        Action<Context, string, Exception> onError = (_, _, exc) => { exceptionFromCacheProvider = exc; };

        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue, onError);

        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;

        // Even though value is in cache, get will error; so value is returned from execution.
        cache.Execute(_ =>
            {
                delegateExecuted = true;
                return ValueToReturnFromExecution;
            }, new Context(OperationKey))
            .Should().Be(ValueToReturnFromExecution);
        delegateExecuted.Should().BeTrue();

        // And error should be captured by onError delegate.
        exceptionFromCacheProvider.Should().Be(ex);
    }

    [Fact]
    public void Should_call_onError_delegate_if_cache_put_errors()
    {
        Exception ex = new Exception();
        ISyncCacheProvider stubCacheProvider = new StubErroringCacheProvider(getException: null, putException: ex);

        Exception? exceptionFromCacheProvider = null;

        const string ValueToReturn = "valueToReturn";
        const string OperationKey = "SomeOperationKey";

        Action<Context, string, Exception> onError = (_, _, exc) => { exceptionFromCacheProvider = exc; };

        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue, onError);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        cache.Execute(_ => ValueToReturn, new Context(OperationKey)).Should().Be(ValueToReturn);

        // error should be captured by onError delegate.
        exceptionFromCacheProvider.Should().Be(ex);

        // failed to put it in the cache
        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeFalse();
        fromCache2.Should().BeNull();

    }

    [Fact]
    public void Should_execute_oncacheget_after_got_from_cache()
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

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy cache = Policy.Cache(stubCacheProvider, new RelativeTtl(TimeSpan.MaxValue), DefaultCacheKeyStrategy.Instance, onCacheAction, emptyDelegate, emptyDelegate, noErrorHandling, noErrorHandling);
        stubCacheProvider.Put(OperationKey, ValueToReturnFromCache, new Ttl(TimeSpan.MaxValue));

        bool delegateExecuted = false;
        cache.Execute(_ =>
            {
                delegateExecuted = true;
                return ValueToReturnFromExecution;
            }, contextToExecute)
            .Should().Be(ValueToReturnFromCache);
        delegateExecuted.Should().BeFalse();

        contextPassedToDelegate.Should().BeSameAs(contextToExecute);
        keyPassedToDelegate.Should().Be(OperationKey);
    }

    [Fact]
    public void Should_execute_oncachemiss_and_oncacheput_if_cache_does_not_hold_value_and_put()
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

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy cache = Policy.Cache(stubCacheProvider, new RelativeTtl(TimeSpan.MaxValue), DefaultCacheKeyStrategy.Instance, emptyDelegate, onCacheMiss, onCachePut, noErrorHandling, noErrorHandling);

        (bool cacheHit1, object? fromCache1) = stubCacheProvider.TryGet(OperationKey);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        cache.Execute(_ => ValueToReturn, contextToExecute).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = stubCacheProvider.TryGet(OperationKey);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);

        contextPassedToOnCacheMiss.Should().BeSameAs(contextToExecute);
        keyPassedToOnCacheMiss.Should().Be(OperationKey);

        contextPassedToOnCachePut.Should().BeSameAs(contextToExecute);
        keyPassedToOnCachePut.Should().Be(OperationKey);
    }

    [Fact]
    public void Should_execute_oncachemiss_but_not_oncacheput_if_cache_does_not_hold_value_and_returned_value_not_worth_caching()
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

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy cache = Policy.Cache(stubCacheProvider, new RelativeTtl(TimeSpan.Zero), DefaultCacheKeyStrategy.Instance, emptyDelegate, onCacheMiss, onCachePut, noErrorHandling, noErrorHandling);

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(OperationKey);
        cacheHit.Should().BeFalse();
        fromCache.Should().BeNull();

        cache.Execute(_ => ValueToReturn, contextToExecute).Should().Be(ValueToReturn);

        contextPassedToOnCacheMiss.Should().BeSameAs(contextToExecute);
        keyPassedToOnCacheMiss.Should().Be(OperationKey);

        contextPassedToOnCachePut.Should().BeNull();
        keyPassedToOnCachePut.Should().BeNull();
    }

    [Fact]
    public void Should_not_execute_oncachemiss_if_dont_query_cache_because_cache_key_not_set()
    {
        string valueToReturn = Guid.NewGuid().ToString();

        Action<Context, string, Exception> noErrorHandling = (_, _, _) => { };
        Action<Context, string> emptyDelegate = (_, _) => { };

        bool onCacheMissExecuted = false;
        Action<Context, string> onCacheMiss = (_, _) => { onCacheMissExecuted = true; };

        CachePolicy cache = Policy.Cache(new StubCacheProvider(), new RelativeTtl(TimeSpan.MaxValue), DefaultCacheKeyStrategy.Instance, emptyDelegate, onCacheMiss, emptyDelegate, noErrorHandling, noErrorHandling);

        cache.Execute(() => valueToReturn /*, no operation key */).Should().Be(valueToReturn);

        onCacheMissExecuted.Should().BeFalse();
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
