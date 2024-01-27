namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class GenericCacheProviderSpecs : IDisposable
{
    [Fact]
    public void Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
    {
        const string OperationKey = "SomeOperationKey";

        bool onErrorCalled = false;
        Action<Context, string, Exception> onError = (_, _, _) => { onErrorCalled = true; };

        ISyncCacheProvider stubCacheProvider = new StubCacheProvider();
        CachePolicy cache = Policy.Cache(stubCacheProvider, TimeSpan.MaxValue, onError);

        (bool cacheHit, object? fromCache) = stubCacheProvider.TryGet(OperationKey);
        cacheHit.Should().BeFalse();
        fromCache.Should().BeNull();

        ResultPrimitive result = cache.Execute(_ => ResultPrimitive.Substitute, new Context(OperationKey));

        onErrorCalled.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_delegate_and_put_value_in_cache_for_non_nullable_types_if_cache_does_not_hold_value()
    {
        const ResultPrimitive ValueToReturn = ResultPrimitive.Substitute;
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

    public void Dispose() =>
        SystemClock.Reset();
}
