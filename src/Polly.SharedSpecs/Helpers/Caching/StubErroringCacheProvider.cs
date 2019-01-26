using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Caching;
using Polly.Utilities;

namespace Polly.Specs.Helpers.Caching
{
    internal class StubErroringCacheProvider : ISyncCacheProvider, IAsyncCacheProvider
    {
        private Exception _getException;
        private Exception _putException;

        private StubCacheProvider innerProvider = new StubCacheProvider();

        public StubErroringCacheProvider(Exception getException, Exception putException)
        {
            _getException = getException;
            _putException = putException;
        }

        public (bool, object) TryGet(string key)
        {
            if (_getException != null) throw _getException;
            return innerProvider.TryGet(key);
        }

        public void Put(string key, object value, Ttl ttl)
        {
            if (_putException != null) throw _putException;
            innerProvider.Put(key, value, ttl);
        }

        #region Naive async-over-sync implementation

        // Intentionally naive async-over-sync implementation.  Its purpose is to be the simplest thing to support tests of the CachePolicyAsync and CacheEngineAsync, not to be a usable implementation of IAsyncCacheProvider.  
        public Task<(bool, object)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return Task.FromResult(TryGet(key));
        }

        public Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            Put(key, value, ttl);
            return TaskHelper.EmptyTask;
        }

        #endregion

    }
}
