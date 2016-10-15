namespace Polly.Caching
{
    /// <summary>
    /// Class that provides helper methods for configuring CacheProviders.
    /// </summary>
    public static class CacheProviderExtensions
    {

        /// <summary>
        /// Provides a strongly <typeparamref name="TCacheFormat"/>-typed version of the supplied <see cref="ICacheProvider"/>
        /// </summary>
        /// <typeparam name="TCacheFormat">The type the returned <see cref="ICacheProvider{TResult}"/> will handle.</typeparam>
        /// <param name="nonGenericCacheProvider">The non-generic cache provider to wrap.</param>
        /// <returns>ICacheProvider{TCacheFormat}.</returns>
        public static ICacheProvider<TCacheFormat> As<TCacheFormat>(this ICacheProvider nonGenericCacheProvider)
        {
            return new TypedCacheProvider<TCacheFormat>(nonGenericCacheProvider);
        }
    }
}
