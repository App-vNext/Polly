namespace Polly.Specs.Helpers.Caching;

internal class StubErroringCacheProvider : ISyncCacheProvider, IAsyncCacheProvider
{
    private readonly Exception? _getException;
    private readonly Exception? _putException;

    private readonly StubCacheProvider _innerProvider = new();

    public StubErroringCacheProvider(Exception? getException, Exception? putException)
    {
        _getException = getException;
        _putException = putException;
    }

    public (bool, object?) TryGet(string key)
    {
        if (_getException != null)
            throw _getException;
        return _innerProvider.TryGet(key);
    }

    public void Put(string key, object? value, Ttl ttl)
    {
        if (_putException != null)
            throw _putException;
        _innerProvider.Put(key, value, ttl);
    }

    #region Naive async-over-sync implementation

    // Intentionally naive async-over-sync implementation.  Its purpose is to be the simplest thing to support tests of the CachePolicyAsync and CacheEngineAsync, not to be a usable implementation of IAsyncCacheProvider.
    public Task<(bool, object?)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        Task.FromResult(TryGet(key));

    public Task PutAsync(string key, object? value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        Put(key, value, ttl);
        return TaskHelper.EmptyTask;
    }

    #endregion
}
