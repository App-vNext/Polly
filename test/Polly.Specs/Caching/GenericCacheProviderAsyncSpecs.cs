namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class GenericCacheProviderAsyncSpecs : IDisposable
{
    [Fact]
    public async Task Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
    {
        const string OperationKey = "SomeOperationKey";

        bool onErrorCalled = false;
        Action<Context, string, Exception> onError = (_, _, _) => onErrorCalled = true;

        var stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, onError);

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(OperationKey, TestCancellation.Token, false);
        cacheHit.ShouldBeFalse();
        fromCache.ShouldBeNull();

        ResultPrimitive result = await cache.ExecuteAsync(async _ =>
        {
            await TaskHelper.EmptyTask;
            return ResultPrimitive.Substitute;
        }, [with(OperationKey)]);

        onErrorCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_execute_delegate_and_put_value_in_cache_for_non_nullable_types_if_cache_does_not_hold_value()
    {
        const ResultPrimitive ValueToReturn = ResultPrimitive.Substitute;
        const string OperationKey = "SomeOperationKey";

        var cancellationToken = TestCancellation.Token;
        var stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, cancellationToken, false);
        cacheHit1.ShouldBeFalse();
        fromCache1.ShouldBeNull();

        (await cache.ExecuteAsync(async _ =>
        {
            await TaskHelper.EmptyTask;
            return ResultPrimitive.Substitute;
        }, [with(OperationKey)])).ShouldBe(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, cancellationToken, false);
        cacheHit2.ShouldBeTrue();
        fromCache2.ShouldBe(ValueToReturn);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
