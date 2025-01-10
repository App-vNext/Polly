namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class GenericCacheProviderAsyncSpecs : IDisposable
{
    [Fact]
    public async Task Should_not_error_for_executions_on_non_nullable_types_if_cache_does_not_hold_value()
    {
        const string OperationKey = "SomeOperationKey";

        bool onErrorCalled = false;
        Action<Context, string, Exception> onError = (_, _, _) => { onErrorCalled = true; };

        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue, onError);

        (bool cacheHit, object? fromCache) = await stubCacheProvider.TryGetAsync(OperationKey, CancellationToken.None, false);
        cacheHit.Should().BeFalse();
        fromCache.Should().BeNull();

        ResultPrimitive result = await cache.ExecuteAsync(async _ =>
        {
            await TaskHelper.EmptyTask;
            return ResultPrimitive.Substitute;
        }, new Context(OperationKey));

        onErrorCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Should_execute_delegate_and_put_value_in_cache_for_non_nullable_types_if_cache_does_not_hold_value()
    {
        const ResultPrimitive ValueToReturn = ResultPrimitive.Substitute;
        const string OperationKey = "SomeOperationKey";

        var cancellationToken = CancellationToken.None;
        IAsyncCacheProvider stubCacheProvider = new StubCacheProvider();
        var cache = Policy.CacheAsync(stubCacheProvider, TimeSpan.MaxValue);

        (bool cacheHit1, object? fromCache1) = await stubCacheProvider.TryGetAsync(OperationKey, cancellationToken, false);
        cacheHit1.Should().BeFalse();
        fromCache1.Should().BeNull();

        (await cache.ExecuteAsync(async _ =>
        {
            await TaskHelper.EmptyTask;
            return ResultPrimitive.Substitute;
        }, new Context(OperationKey))).Should().Be(ValueToReturn);

        (bool cacheHit2, object? fromCache2) = await stubCacheProvider.TryGetAsync(OperationKey, cancellationToken, false);
        cacheHit2.Should().BeTrue();
        fromCache2.Should().Be(ValueToReturn);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
